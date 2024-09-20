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
- `appsettings.json` and `logs/` are now git-ignored, so live credentials and
  log output cannot be committed by accident.
- Removed a personal local file path from the sample request in
  `ImageAnalysisAPI.http`.

### Known issues
Tracked in [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#known-technical-debt),
including the inverted `ModifiedExplanation` captions, a divide-by-zero in
`LiveCheckService`, and the Windows-only EXIF reader.

## [0.1.0] — 2024-09-13

### Added
- Initial implementation: EXIF metadata extraction, `Genuine` / `Live` /
  `Modified` heuristic scoring, Azure Blob Storage upload, Azure Document
  Intelligence extraction for identity documents and invoices, Azure Face API
  attribute detection, IP geolocation, and Serilog logging.
