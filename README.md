# Document Analyzer

[![CI](https://github.com/dcosodev/document-analyzer/actions/workflows/ci.yml/badge.svg)](https://github.com/dcosodev/document-analyzer/actions/workflows/ci.yml)
[![Tests](https://img.shields.io/badge/tests-31%20passing-brightgreen)](tests/ImageAnalysisAPI.Tests)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An ASP.NET Core 8 Web API that scores photographs and identity documents for
**authenticity**, **recency** and **tampering**, and extracts their structured
content using Azure AI services.

Given an uploaded image (or a public image URL), the API reads the EXIF
metadata, cross-checks the embedded GPS coordinates against the caller's
IP-derived location and the reported device location, and returns three
heuristic scores together with a plain-language explanation of each. Identity
documents and invoices are additionally run through Azure Document
Intelligence, and selfies through the Azure Face API.

The original use case was a remote-onboarding flow: verifying that a submitted
ID photo was taken by a real camera, at the claimed place, at the claimed time,
and had not been retouched before upload.

---

## Table of contents

- [How it works](#how-it-works)
- [The three scores](#the-three-scores)
- [Quick start](#quick-start)
- [Configuration](#configuration)
- [API reference](#api-reference)
- [Documentation](#documentation)
- [Testing](#testing)
- [Known limitations](#known-limitations)
- [Contributing](#contributing)
- [License](#license)

---

## How it works

```
                 ┌──────────────────────────────────────┐
  multipart/     │      ImageAnalysisController         │
  form-data ────▶│      POST /api/ImageAnalysis/        │
  (files + URLs) │            imageAnalyse              │
                 └───────────────────┬──────────────────┘
                                     │
                          ┌──────────▼───────────┐
                          │ ImageProcessingService│
                          └──────────┬───────────┘
                                     │
        ┌────────────────┬───────────┼────────────┬─────────────────┐
        │                │           │            │                 │
┌───────▼──────┐ ┌───────▼──────┐ ┌──▼─────────┐ ┌▼──────────────┐ ┌▼─────────────┐
│MetadataReader│ │IPGeolocation │ │ Genuine /  │ │ Blob Storage  │ │ Face API  or │
│ EXIF tags    │ │ ip → lat/lon │ │ Live /     │ │ upload → URL  │ │ Document     │
│              │ │              │ │ Modified   │ │               │ │ Intelligence │
└──────────────┘ └──────────────┘ └────────────┘ └───────────────┘ └──────────────┘
                                     │
                          ┌──────────▼───────────┐
                          │  PhotoResponse[]     │
                          │  scores + extraction │
                          └──────────────────────┘
```

Each submitted file is written to a temporary local path, parsed for EXIF
metadata, uploaded to an Azure Blob container so that the Azure AI services can
reach it over a public URL, analysed, and then deleted locally. Failures on a
single image are logged and skipped; the rest of the batch still returns.

## The three scores

All three are percentages derived from EXIF metadata — they are **heuristics,
not forensic proof**. Full derivation in [`docs/SCORING.md`](docs/SCORING.md).

| Score | Question it answers | Signals used |
|---|---|---|
| `Genuine` | Did a real camera produce this file? | Presence of the `Make` and `Model` EXIF tags |
| `Live` | Was it taken here, and just now? | Haversine distance between EXIF GPS, the caller-reported GPS (≤ 10 km) and the IP-derived location (≤ 500 km), plus whether the capture timestamp is under 60 minutes old |
| `Modified` | Has it been through an editor? | Presence of editing-software metadata, and whether the original and modified timestamps differ |

Each score ships with a matching `*Explanation` string, so the response is
directly presentable to an end user or a reviewing agent.

## Quick start

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) and an
Azure subscription with Document Intelligence, Face API and Blob Storage
resources. Runs on Windows, macOS and Linux.

```bash
git clone https://github.com/dcosodev/document-analyzer.git
cd document-analyzer

cp appsettings.example.json appsettings.json
# edit appsettings.json with your Azure endpoints and keys

dotnet restore
dotnet run
```

The API listens on `https://localhost:5001` and `http://localhost:5000`.
In the `Development` environment, interactive OpenAPI docs are served at
<https://localhost:5001/swagger>.

Smoke-test it with the ready-made request in
[`ImageAnalysisAPI.http`](ImageAnalysisAPI.http) (VS Code REST Client or
Visual Studio), or with curl:

```bash
curl -k -X POST https://localhost:5001/api/ImageAnalysis/imageAnalyse \
  -F "type=idpassport" \
  -F "gps=40.712776,-74.005974" \
  -F "clientIP=8.8.8.8" \
  -F "images=@passport.jpg"
```

## Configuration

`appsettings.json` is **git-ignored on purpose** — it holds live credentials.
Copy `appsettings.example.json` and fill it in. Every key below is required;
the application fails fast at startup if one is missing.

| Key | Azure resource |
|---|---|
| `Azure:DocumentIntelligence:Endpoint` / `:Key` | Document Intelligence (formerly Form Recognizer) |
| `Azure:FaceAPI:Endpoint` / `:SubscriptionKey` | Face API |
| `Azure:Storage:ConnectionString` | Blob Storage account |
| `IPGeolocation:Endpoint` / `:ApiKey` | [ipgeolocation.io](https://ipgeolocation.io) |

For deployments, prefer environment variables over the file — ASP.NET Core
maps `Azure__Storage__ConnectionString` onto `Azure:Storage:ConnectionString`
automatically. Full reference in
[`docs/CONFIGURATION.md`](docs/CONFIGURATION.md).

## API reference

### `POST /api/ImageAnalysis/imageAnalyse`

`Content-Type: multipart/form-data`

| Field | Type | Required | Description |
|---|---|---|---|
| `images` | file[] | one of `images`/`paths` | Image files to analyse |
| `paths` | string[] | one of `images`/`paths` | Absolute URLs to publicly reachable images |
| `type` | string | yes | Analysis mode — see below |
| `gps` | string | no | Device location as `"lat,lon"`, used for the `Live` score |
| `clientIP` | string | no | Caller IP, resolved to a coarse location |
| `clientCamera` | bool | no | Whether the capture came from an in-app camera |

**`type` values:** `face` routes to the Face API; `idpassport`, `idfront`,
`idback` and `passport` route to the `prebuilt-idDocument` model; `invoice`
routes to `prebuilt-invoice`. Any other value returns `"Unrecognized type"` in
the extraction field rather than an error.

Returns `200` with a `PhotoResponse[]`. Errors return an `ErrorResponse`:
`400` for invalid arguments or malformed URIs, `503` when an upstream HTTP call
fails, `500` otherwise.

Field-by-field response schema and worked examples in
[`docs/API.md`](docs/API.md).

## Documentation

| Document | Contents |
|---|---|
| [`docs/API.md`](docs/API.md) | Endpoint contract, response schema, examples |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Layering, request lifecycle, service responsibilities |
| [`docs/CONFIGURATION.md`](docs/CONFIGURATION.md) | Every setting, environment-variable form, Azure setup |
| [`docs/SCORING.md`](docs/SCORING.md) | How each score is computed, and what it cannot tell you |

## Testing

```bash
dotnet test
```

33 tests cover the three scoring services, coordinate handling in the metadata
reader and request validation, including regression tests for the defects listed
in the [changelog](CHANGELOG.md). CI runs them on Ubuntu and Windows on every
push.

The scorers are pure functions over primitives, so they are tested directly.
The Azure-backed services are not covered — exercising them requires live
credentials, and no integration suite exists yet.

## Known limitations

This project is a working prototype, and it is more useful documented honestly
than oversold:

- **The scores are heuristics.** A `Genuine` score of 100% means two EXIF tags
  were present, which any metadata editor can forge. Treat the output as a
  triage signal, not evidence.
- **EXIF timestamps carry no timezone.** The recency check compares against
  UTC without reading the `OffsetTimeDigitized` tag, so a genuinely fresh
  photo taken outside UTC can lose the recency point.
- **The endpoint is unauthenticated.** There is no API key or rate limiting. Do
  not expose it publicly as-is.
- **Request limits are enforced.** Each request is limited to 50 MB, with at
  most 10 non-empty inputs and 10 MB per uploaded file.
- **Uploads use private blob storage.** Files are exposed to Azure AI only
  through a read-only SAS URL valid for one hour and are deleted after analysis.
  If deletion fails, configure Azure Storage lifecycle policies as a second
  control.
- **Only pure logic and request validation are tested.** The Azure integrations,
  controller and storage services have no automated integration coverage.

## Contributing

Issues and pull requests are welcome — see
[`CONTRIBUTING.md`](CONTRIBUTING.md). Security reports should follow
[`SECURITY.md`](SECURITY.md) rather than the public issue tracker.

## License

[MIT](LICENSE) © dcosodev
