# Contributing

Thanks for taking an interest. Issues and pull requests are both welcome.

## Getting set up

```bash
git clone https://github.com/dcosodev/document-analyzer.git
cd document-analyzer
cp appsettings.example.json appsettings.json   # fill in your Azure credentials
dotnet restore
dotnet build
dotnet run
```

See [`docs/CONFIGURATION.md`](docs/CONFIGURATION.md) for what each setting
does and which Azure resources you need.

Run the tests with:

```bash
dotnet test
```

The project builds and runs on Windows, macOS and Linux.

## Where to start

[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#known-technical-debt) lists the
known gaps, roughly in order of value. The most useful ones:

- Add authentication and rate limiting to the endpoint.
- Replace public blob access with short-lived SAS tokens, and prefix blob names
  with a GUID so concurrent uploads cannot overwrite each other.
- Read the `OffsetTimeDigitized` EXIF tag so the recency check stops penalising
  photos taken outside UTC.
- Collapse the duplicated file and URL branches of
  `ImageProcessingService.AnalyzeImages`.
- Add integration coverage for the Azure-backed services.

## Pull requests

- Branch from `main`.
- Keep the change focused; unrelated cleanups belong in their own PR.
- `dotnet build --configuration Release` and `dotnet test` must pass. CI runs
  both on Ubuntu and Windows.
- New behaviour needs a test. The scorers are pure functions, so this is
  usually a few lines in `tests/ImageAnalysisAPI.Tests`.
- Match the surrounding code style. An `.editorconfig` is in the repository and
  standard .NET conventions apply: PascalCase for members, `_camelCase` for
  private fields, `async`/`await` over blocking calls.
- Update the relevant document under `docs/` in the same PR when you change
  behaviour. Documentation drifting out of sync with the code is the specific
  problem this repository has already had once.
- Never commit `appsettings.json`, a key, or a connection string.

## Reporting bugs

Open an issue with the request you sent, the response or exception you got, and
what you expected. Redact credentials and any real identity document.

Security problems go to [`SECURITY.md`](SECURITY.md), not the issue tracker.
