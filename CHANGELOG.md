# Changelog

All notable changes to this project are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Full documentation set under `docs/`: API reference, architecture,
  configuration, and the derivation of the three scores.
- `appsettings.example.json` template — the application previously could not be
  started from a fresh clone, because no configuration file was tracked.
- Continuous integration on GitHub Actions, building on Ubuntu and Windows.
- `CONTRIBUTING.md`, `SECURITY.md`, `CODE_OF_CONDUCT.md` and this changelog.
- Swagger UI at `/swagger` in the `Development` environment. Swashbuckle was
  already a dependency but had never been wired into the pipeline.
- `.editorconfig` describing the project's C# conventions.

### Changed
- Rewrote `README.md`. The previous version documented an endpoint
  (`/api/ImageAnalysis/analyze`) that does not exist — the actual route is
  `/api/ImageAnalysis/imageAnalyse` — and omitted the required
  `Azure:FaceAPI` settings, without which the service throws at startup.
- Documented the `type` values the code actually accepts (`face`, `idpassport`,
  `idfront`, `idback`, `passport`, `invoice`).

### Fixed
- **`modifiedExplanation` was inverted.** `ModifiedCheckService` awards points
  for the *absence* of tampering signals, so `100` means clean — but the
  caption for `100` read *"An editing program and an editing time have been
  detected."* The `0` and `100` captions are now the right way round. Integrators
  relying on the old strings should re-check their mapping.
- **`LiveCheckService` returned `NaN`.** When neither location comparison could
  run, `locFactors` stayed at `0` and the score divided by it, serialising as
  `null`. The location component now contributes `0`. Coordinate parsing is
  validated for shape and uses `TryParse` rather than catching `FormatException`.
- **EXIF reading no longer requires Windows.** `System.Drawing.Common` has been
  Windows-only since .NET 7, so metadata extraction threw
  `PlatformNotSupportedException` on Linux and macOS.  `MetadataReaderUtil` now
  uses `MetadataExtractor`, and the service is cross-platform.
- **GPS hemisphere references are honoured.** The previous reader ignored the
  N/S and E/W reference tags, so coordinates south of the equator or west of
  Greenwich came back with the wrong sign.
- **Editing software is read from the correct tag.** `Software` (`0x0131`) is
  what editors write; `UserComment` (`0x9286`) is kept as a fallback.
- **Dropped two accidental EXIF mappings** — ISO speed onto `LocationDetails`
  and image unique ID onto `GpsLocMeta`.
- `appsettings.json`, `logs/` and test results are now git-ignored, so live
  credentials and log output cannot be committed by accident.
- Removed a personal local file path from the sample request in
  `ImageAnalysisAPI.http`.

- `tests/ImageAnalysisAPI.Tests` (xUnit), covering the three scoring services
  and coordinate handling, run on Ubuntu and Windows in CI.

### Removed
- `System.Drawing.Common`, replaced by `MetadataExtractor`.
- The `Console.WriteLine` diagnostics in `IPGeolocationUtil` and
  `LiveCheckService`. One of them printed the full ipgeolocation request URI,
  including the API key. Location diagnostics now go through `ILogger` at
  `Debug` level.

### Known issues
Remaining gaps are tracked in
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#known-technical-debt): the
endpoint is unauthenticated, uploaded blobs are publicly readable and never
expire, and EXIF timezone offsets are still ignored.

## [0.1.0] — 2024-09-13

### Added
- Initial implementation: EXIF metadata extraction, `Genuine` / `Live` /
  `Modified` heuristic scoring, Azure Blob Storage upload, Azure Document
  Intelligence extraction for identity documents and invoices, Azure Face API
  attribute detection, IP geolocation, and Serilog logging.
