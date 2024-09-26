# Security policy

## Reporting a vulnerability

Please **do not open a public issue** for a security problem.

Report it through
[GitHub private vulnerability reporting](https://github.com/dcosodev/document-analyzer/security/advisories/new),
which notifies the maintainer directly. Include what you found, how to
reproduce it, and what an attacker could achieve. Expect an acknowledgement
within a few days.

## Supported versions

This is a prototype with no released versions. Fixes land on `main`.

## Known weaknesses — read before deploying

This project is published as a portfolio and reference implementation. It is
**not hardened for production**, and the following are known and unfixed. They
are documented rather than hidden:

| Weakness | Impact |
|---|---|
| No authentication or authorization on the endpoint | Anyone who can reach the service can submit analyses and consume your Azure quota |
| No rate limiting, upload-size cap or batch limit | Trivially abusable for cost amplification and memory pressure |
| Blob container created with `PublicAccessType.Blob` | Every uploaded identity document is readable by anyone with the URL |
| Uploaded blobs are never deleted | Personal data accumulates indefinitely with no retention policy |
| Blob names are the original file names, uploaded with `overwrite: true` | Two users submitting `passport.jpg` overwrite each other's document |
| `paths` URLs are fetched server-side without an allow-list | Server-side request forgery against internal network addresses |
| Serilog writes blob URLs to `logs/` | Log files are as sensitive as the documents themselves |

If you intend to run this against real data, the minimum work is:
authentication on the endpoint, short-lived SAS tokens instead of public blob
access, GUID-prefixed blob names, a container retention policy, and an
allow-list for `paths`.

## Handling credentials

`appsettings.json` is git-ignored and holds live keys. Use environment
variables in any deployed environment — see
[`docs/CONFIGURATION.md`](docs/CONFIGURATION.md). If you believe a key of yours
was committed anywhere, rotate it in the Azure portal; removing the commit is
not sufficient.
