# Architecture

A single-project ASP.NET Core 8 Web API, organised into four layers.

```
Controllers/   HTTP surface, exception-to-status-code mapping
Services/      Orchestration, scoring, Azure clients, storage
Utils/         Stateless helpers — EXIF, geolocation, DI wiring
Models/        Request and response DTOs
```

Dependencies flow inward: controllers depend on services through
`IImageProcessingService`, services depend on utils, and nothing depends on
controllers.

---

## Request lifecycle

1. **`ImageAnalysisController.AnalyzeImages`** accepts the multipart form and
   normalises null arrays to empty ones. It owns all exception-to-status
   mapping — `ArgumentException` and `UriFormatException` become `400`,
   `HttpRequestException` becomes `503`, everything else `500`.

2. **`ValidationService`** rejects the request when neither a non-empty file
   nor a non-empty path was supplied.

3. **`ImageProcessingService`** iterates uploaded files, then URLs. For each
   item it:
   - persists the bytes to a temporary local file (`FileStorageLocalService`,
     or a direct `HttpClient` download for URLs);
   - reads EXIF via `MetadataReaderUtil`;
   - builds the scored response via `PhotoResponsesService`;
   - uploads the file to Blob Storage (`FileStorageAzureService`) to obtain a
     publicly fetchable URL;
   - dispatches to `FaceRecognitionService` when `type` is `face`, otherwise
     to `FormRecognizerService`;
   - deletes the local temp file in a `finally` block.

   A failure on one item is logged and skipped; the batch continues.

4. **`PhotoResponsesService`** is the scoring composition root. It calls
   `OCRDataExtractService`, re-reads the metadata, resolves the caller IP
   through `IPGeolocationUtil`, then invokes the three scorers and attaches a
   human-readable explanation to each result.

## Why images round-trip through Blob Storage

Azure Document Intelligence and the Face API are called through their
`...FromUri` overloads, which require a URL the Azure service can fetch
itself. Uploading to a blob container is what turns a local upload into such a
URL. This is the reason `FileStorageAzureService` creates its container with
`PublicAccessType.Blob`, and it is the single largest privacy weakness in the
design — see below.

## Service responsibilities

| Service | Responsibility |
|---|---|
| `ImageProcessingService` | Batch orchestration, temp-file lifecycle, routing by `type` |
| `PhotoResponsesService` | Assembles a `PhotoResponse`: scores + explanations + extraction |
| `ValidationService` | Input presence check |
| `GenuineCheckService` | EXIF camera-tag score |
| `LiveCheckService` | Haversine distance + recency score |
| `ModifiedCheckService` | Editing-metadata score |
| `OCRDataExtractService` | Raw text extraction |
| `FormRecognizerService` | Document Intelligence prebuilt models |
| `FaceRecognitionService` | Face API attribute detection |
| `FileStorageLocalService` | Temp-file write and delete |
| `FileStorageAzureService` | Blob upload and URL resolution |

All are registered as `Scoped` in `Program.cs`; `BlobServiceClient` is a
`Singleton` built from configuration, and throws at startup when the
connection string is absent.

## Cross-cutting concerns

- **Logging** — Serilog, writing to the console and to a daily rolling file
  under `logs/`. Injected as `ILogger<T>` throughout.
- **Serialization** — Newtonsoft.Json with `ReferenceLoopHandling.Ignore`.
- **API docs** — Swashbuckle, exposed at `/swagger` in `Development` only.

---

## Testing

`tests/ImageAnalysisAPI.Tests` (xUnit) covers the three scoring services and
the coordinate handling in `MetadataReaderUtil`. They are pure functions over
primitives, so no test doubles are needed beyond `NullLogger<T>`.
`InternalsVisibleTo` exposes the internal helpers `CalculateDistance`,
`IsRecent` and `FormatCoordinates` to the test assembly.

The API project sits at the repository root, so its default source glob is
explicitly told to skip `tests/**` in `ImageAnalysisAPI.csproj`.

Not covered: the controller, the Azure-backed services and blob storage. Those
need live credentials, and an integration suite behind a CI secret would be the
next step.

---

## Known technical debt

Recorded deliberately: this is a prototype, and the gaps are worth naming.

### Correctness

- **EXIF timezone handling.** Capture timestamps are compared against
  `DateTime.UtcNow` without reading the `OffsetTimeDigitized` tag, penalising
  legitimate photos taken outside UTC.

### Security

- **No authentication or rate limiting** on the endpoint, and no cap on upload
  size or batch length.
- **Public blob container, no lifecycle.** Uploaded identity documents are
  world-readable by URL and are never deleted. Short-lived SAS tokens plus a
  container expiry policy are the correct design.
- **Blob names collide.** `StoreFileAzure` uses the original file name as the
  blob name and uploads with `overwrite: true`, so two callers submitting
  `passport.jpg` overwrite each other's document. Names should be prefixed with
  a GUID.
- **`HttpClient` is constructed per request** in `ImageProcessingService` and
  `IPGeolocationUtil`, risking socket exhaustion. `IHttpClientFactory` is the
  supported pattern.
- **Unvalidated URL fetching.** `paths` values are downloaded server-side with
  no allow-list, which is a server-side request forgery vector.

### Structure

- **Duplicated registration helper.** `Utils/BlobStorageConfig.cs` and
  `Utils/ServiceCollectionExtensions.cs` define the same
  `AddBlobServiceClient` extension, and neither is called — `Program.cs`
  registers the client inline. Two of the three should go.
- **`ApiKeyServiceClientCredentials` is declared twice**, in
  `Services/FaceRecognitionService.cs` and `Utils/VCConfigUtil.cs`.
- **`VCConfigUtil` re-reads `appsettings.json` from disk** instead of using the
  injected `IConfiguration`, bypassing environment-variable overrides.
- **The two branches of `ImageProcessingService.AnalyzeImages`** — files and
  URLs — are near-identical copies of ~60 lines. They should converge once
  both paths produce a `FileInfo`.
- **No integration coverage.** The scorers are tested; the Azure paths,
  controller and storage services are not.

## Resolved

Kept for context, since earlier revisions of this document listed them as open:

- The inverted `ModifiedExplanation` captions were swapped.
- `LiveCheckService` no longer divides by zero when no location comparison can
  run.
- `System.Drawing.Common` was replaced with `MetadataExtractor`, making the
  service cross-platform and fixing GPS hemisphere signs along the way.
- The accidental EXIF mappings (ISO speed onto `LocationDetails`, image unique
  ID onto `GpsLocMeta`) were removed.
- The `Console.WriteLine` diagnostics were removed, including the one that
  printed the ipgeolocation API key.
