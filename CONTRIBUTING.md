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

**On macOS and Linux** the project builds and starts, but EXIF reading throws
`PlatformNotSupportedException` because `System.Drawing.Common` is
Windows-only from .NET 7 onward. Document extraction still works; the three
scores do not. Replacing the metadata reader is the change that would fix
this, and it is a good first contribution.

## Where to start

[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#known-technical-debt) lists the
known gaps, roughly in order of value. The most useful ones:

- Swap the inverted `ModifiedExplanation` captions.
- Short-circuit the `LiveCheckService` divide-by-zero to `0`.
- Replace `System.Drawing.Common` with a cross-platform EXIF reader.
- Add a test project — the three scorers are pure functions and are the
  natural place to begin.

## Pull requests

- Branch from `main`.
- Keep the change focused; unrelated cleanups belong in their own PR.
- `dotnet build --configuration Release` must pass. CI runs it on Ubuntu and
  Windows.
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
