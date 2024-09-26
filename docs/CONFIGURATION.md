# Configuration

All settings are read through `IConfiguration`, so any ASP.NET Core provider
works. Two are practical here: a local `appsettings.json` for development, and
environment variables for deployment.

`appsettings.json` is **git-ignored** — it holds live credentials and must
never be committed. `appsettings.example.json` is the tracked template.

```bash
cp appsettings.example.json appsettings.json
```

---

## Settings reference

| Key | Environment variable | Required | Description |
|---|---|---|---|
| `Azure:DocumentIntelligence:Endpoint` | `Azure__DocumentIntelligence__Endpoint` | yes | Document Intelligence resource endpoint |
| `Azure:DocumentIntelligence:Key` | `Azure__DocumentIntelligence__Key` | yes | Its access key |
| `Azure:FaceAPI:Endpoint` | `Azure__FaceAPI__Endpoint` | yes | Face API resource endpoint |
| `Azure:FaceAPI:SubscriptionKey` | `Azure__FaceAPI__SubscriptionKey` | yes | Its subscription key |
| `Azure:Storage:ConnectionString` | `Azure__Storage__ConnectionString` | yes | Storage account connection string |
| `IPGeolocation:Endpoint` | `IPGeolocation__Endpoint` | yes | `https://api.ipgeolocation.io/ipgeo` |
| `IPGeolocation:ApiKey` | `IPGeolocation__ApiKey` | yes | ipgeolocation.io API key |
| `Logging:LogLevel:Default` | `Logging__LogLevel__Default` | no | Defaults to `Information` |
| `AllowedHosts` | `AllowedHosts` | no | Defaults to `*` |

The double underscore is how ASP.NET Core encodes the `:` separator in
environment-variable names.

### Fail-fast behaviour

Missing configuration surfaces at startup, not on the first request:

- `BlobServiceClient` registration throws `ArgumentNullException` when
  `Azure:Storage:ConnectionString` is absent.
- `FormRecognizerService` and `FaceRecognitionService` throw in their
  constructors when their endpoint or key is missing.

> `VCConfigUtil` currently loads `appsettings.json` directly from disk rather
> than through `IConfiguration`, so environment-variable overrides do not reach
> it. See [ARCHITECTURE.md](ARCHITECTURE.md#structure).

---

## Azure resources

Three resources are needed. All are available on consumption or free tiers
sufficient for evaluation.

### 1. Document Intelligence

Create an **Azure AI Document Intelligence** resource (formerly Form
Recognizer). Copy its endpoint and one of its two keys. The prebuilt
`prebuilt-idDocument` and `prebuilt-invoice` models are used; no custom model
training is required.

### 2. Face API

Create an **Azure AI Face** resource and copy its endpoint and subscription
key.

> Access to Face API attribute detection is **gated**. Microsoft requires an
> approved Limited Access application for face attributes including age and
> emotion. Without approval the service returns `403 Forbidden`, which
> `FaceRecognitionService` logs explicitly. Document analysis (`idpassport`,
> `invoice`) does not depend on this and works regardless.

### 3. Blob Storage

Create a **Storage account** and copy its connection string. The container
`validator` is created automatically on first upload, with
`PublicAccessType.Blob`, because the Azure AI services fetch each image over an
anonymous URL.

> This makes uploaded documents world-readable to anyone holding the URL, and
> nothing deletes them. Use a dedicated throwaway storage account for
> evaluation, and read
> [ARCHITECTURE.md](ARCHITECTURE.md#security) before considering any real
> deployment.

### 4. ipgeolocation.io

Register at [ipgeolocation.io](https://ipgeolocation.io) for a free API key.
This resolves `clientIP` to coordinates for the `Live` score.

---

## Local run profiles

`Properties/launchSettings.json` defines two profiles:

| Profile | URLs |
|---|---|
| `ImageAnalysisAPI` (`dotnet run`) | `https://localhost:5001`, `http://localhost:5000` |
| `IIS Express` | `http://localhost:5279`, SSL on `44321` |

Both set `ASPNETCORE_ENVIRONMENT=Development`, which enables the developer
exception page and the `/swagger` UI.

## Logging

Serilog writes to the console and to `logs/log-<date>.txt`, rolling daily. The
`logs/` directory is git-ignored. Log output includes blob URLs of uploaded
documents, so treat the log files as sensitive. API keys are never logged.

Distance and score diagnostics from `LiveCheckService` are emitted at `Debug`
level; set `Logging:LogLevel:Default` to `Debug` to see them.
