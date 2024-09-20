# Scoring

Three independent percentages are computed per image, entirely from EXIF
metadata and the caller-supplied context. No pixel analysis is involved.

They are **triage heuristics, not forensic evidence.** Every signal below lives
in metadata that a determined actor can write by hand.

---

## `Genuine` — did a real camera produce this file?

`GenuineCheckService.CheckIfGenuine(cameraMake, cameraModel)`

Two factors, each worth one point:

| Signal | Point awarded when |
|---|---|
| EXIF `Make` (`0x010F`) | non-empty |
| EXIF `Model` (`0x0110`) | non-empty |

`score = (points / 2) × 100`

| Value | Meaning |
|---|---|
| 100 | Camera maker and model both recognised |
| 50 | One of the two is missing |
| 0 | Neither is present |

Screenshots, images re-encoded by messaging apps, and files stripped by a
privacy tool all score 0 — as does a photograph that is entirely authentic but
passed through software that discards metadata. A low `Genuine` score is a
reason to look closer, not a verdict.

---

## `Live` — was it taken here, and just now?

`LiveCheckService.CheckIfLive(gpsDateTime, gpsNow, gpsLocMeta, geoDetails)`

Combines a **location** component and a **recency** component.

### Location

Up to two comparisons run, each contributing one point and one factor:

| Comparison | Point awarded when |
|---|---|
| EXIF GPS vs. the caller-reported `gps` | great-circle distance ≤ **10 km** |
| EXIF GPS vs. the IP-derived location | great-circle distance ≤ **500 km** |

Distances use the haversine formula with `R = 6371 km`. Coordinates that fail
to parse score zero for that comparison. The looser IP threshold reflects that
IP geolocation resolves to a city or region, not a street.

### Recency

One point when the EXIF digitised timestamp (`0x9004`, parsed as
`yyyy:MM:dd HH:mm:ss`) is within **60 minutes** of `DateTime.UtcNow`.

### Combination

```
live = ((locScore / locFactors) + dateScore) / 2 × 100
```

| Value | Meaning |
|---|---|
| 100 | Location within range and photo is recent |
| 75 | One location comparison failed; photo is recent |
| 50 | Location out of range, or photo older than an hour |
| 25 | One location comparison failed and photo is old |
| 0 | No usable location, and photo older than an hour |

> **Timezone caveat.** The EXIF timestamp is compared against `UtcNow`, but
> EXIF `DateTimeDigitized` carries no timezone and is conventionally local
> time. A genuinely fresh photo taken outside UTC will therefore lose the
> recency point. Correcting this requires reading the `OffsetTimeDigitized`
> tag, which the current metadata reader does not.

> **Division by zero.** When neither location comparison can run — no
> caller-supplied `gps` and no IP-derived location — `locFactors` stays `0`
> and the expression yields `NaN` rather than `0`. See
> [ARCHITECTURE.md](ARCHITECTURE.md#known-technical-debt).

---

## `Modified` — has it been through an editor?

`ModifiedCheckService.CheckIfModified(editingSoftware, dateTimeOriginal, dateTimeModified)`

Two factors, each worth one point:

| Signal | Point awarded when |
|---|---|
| Editing-software metadata (EXIF `0x9286`) | **absent** |
| Original vs. modified timestamps | **equal**, or the original is absent |

`score = (points / 2) × 100`

### A note on the `Modified` score

The scale runs opposite to what the name suggests: **100 means no evidence of
modification was found**, 0 means both tampering signals fired. Read it as a
"clean" score rather than a "modified" score.

The explanation strings returned in `modifiedExplanation` currently describe
the inverse — `100` is captioned *"An editing program and an editing time have
been detected."* This is a known defect, tracked in
[ARCHITECTURE.md](ARCHITECTURE.md#known-technical-debt); rely on the numeric
value, not the caption, until it is resolved.

---

## What these scores cannot tell you

- **Metadata is writeable.** `exiftool` can set any tag in seconds. All three
  scores describe what the file *claims*, not what is true.
- **No pixel-level forensics.** There is no error-level analysis, no noise or
  sensor-pattern check, no generative-image detection. A high-quality synthetic
  image with fabricated EXIF scores 100 across the board.
- **Absence is not guilt.** Legitimate pipelines strip metadata routinely.
  Low scores correlate at least as strongly with privacy-conscious tooling as
  with fraud.

Use the output to rank a review queue. Do not use it to reject a person.
