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

## Known technical debt

Recorded deliberately: this is a prototype, and the gaps are worth naming.

### Correctness

- **`ModifiedExplanation` is inverted.** `ModifiedCheckService` awards points
  for the *absence* of tampering signals, so `100` means "clean", but the
  caption for `100` reads *"An editing program and an editing time have been
  detected."* The captions for `0` and `100` need to be swapped.
- **`LiveCheckService` can divide by zero.** When neither location comparison
  runs, `locFactors` is `0` and the score becomes `NaN`. It should short-circuit
  to `0`.
- **EXIF timezone handling.** Capture timestamps are compared against
  `DateTime.UtcNow` without reading the offset tags, penalising legitimate
  photos taken outside UTC.
- **Overloaded EXIF tags.** `MetadataReaderUtil` maps ISO speed (`0x8827`) onto
  `LocationDetails` and image unique ID (`0xA420`) onto `GpsLocMeta`, which are
  then overwritten when real GPS tags exist. These mappings look accidental.

### Portability

- **`System.Drawing.Common` is Windows-only** from .NET 7 onward. The project
  builds on any platform but EXIF reading throws
  `PlatformNotSupportedException` on Linux and macOS. Replacing
  `MetadataReaderUtil` with `MetadataExtractor` or `ImageSharp` would make the
  whole service cross-platform and is the highest-value single change here.

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
- **`Console.WriteLine` diagnostics** remain in `LiveCheckService` and
  `IPGeolocationUtil` alongside the Serilog logger, and the latter prints the
  full request URI including the API key.
- **No test project.** CI verifies compilation only. The three scorers are pure
  functions over primitives and would be straightforward to cover first.
