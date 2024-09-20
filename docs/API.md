# API reference

Base URL (local development): `https://localhost:5001`

The API exposes a single endpoint. In the `Development` environment an
interactive OpenAPI UI is available at `/swagger`.

---

## `POST /api/ImageAnalysis/imageAnalyse`

Analyses one or more images and returns a score set plus extracted content for
each.

**Content type:** `multipart/form-data`

### Request fields

| Field | Type | Required | Description |
|---|---|---|---|
| `images` | `file[]` | at least one of `images` / `paths` | Image files uploaded directly. Zero-length files are skipped. |
| `paths` | `string[]` | at least one of `images` / `paths` | Absolute, publicly reachable image URLs. Values that are not well-formed absolute URIs are logged and skipped. |
| `type` | `string` | yes | Analysis mode, see [Analysis types](#analysis-types). |
| `gps` | `string` | no | Device location at capture time, `"lat,lon"` in decimal degrees, invariant culture. Feeds the `Live` score. |
| `clientIP` | `string` | no | Caller's IP address, resolved to a coarse location through ipgeolocation.io. Feeds the `Live` score. |
| `clientCamera` | `bool` | no | Flag indicating the image came from an in-app camera rather than the gallery. Currently accepted and forwarded but not used in scoring. |

Both `images` and `paths` may be supplied in the same request; results are
concatenated, files first.

### Analysis types

| `type` | Route taken | Azure model |
|---|---|---|
| `face` | Face API | `Face.DetectWithUrl` with age, gender, smile, emotion, glasses, hair, occlusion, blur, exposure and noise attributes |
| `idpassport`, `idfront`, `idback`, `passport` | Document Intelligence | `prebuilt-idDocument` |
| `invoice` | Document Intelligence | `prebuilt-invoice` |
| anything else | Document Intelligence | none — the `FormRecognizer` field is set to `"Unrecognized type"` |

Comparison is case-insensitive.

### Response — `200 OK`

An array of `PhotoResponse` objects, one per successfully processed image.

| Field | Type | Source |
|---|---|---|
| `photoName` | string | File name of the analysed image |
| `errorMessage` | string | Populated when EXIF parsing failed for this image |
| `cameraMake` | string | EXIF `0x010F` |
| `cameraModel` | string | EXIF `0x0110` |
| `gpsLocMeta` | string | EXIF GPS latitude/longitude as `"lat,lon"` |
| `gpsNow` | string | Echo of the request `gps` field |
| `clientIP` | string | Echo of the request `clientIP` field |
| `gpsDateTime` | string | EXIF `0x9004` (digitised timestamp), format `yyyy:MM:dd HH:mm:ss` |
| `datetimeOriginal` | string | EXIF `0x9003` |
| `datetimeModified` | string | EXIF `0x0132` |
| `editingSoftware` | string | EXIF `0x9286` (user comment) |
| `locationDetails` | string | Resolved place description |
| `countryCode` | string | ISO country code from the IP lookup |
| `genuine` | number | 0–100, see [SCORING.md](SCORING.md) |
| `live` | number | 0–100 |
| `modified` | number | 0–100 |
| `genuineExplanation` | string | Human-readable reason for the `genuine` value |
| `liveExplanation` | string | Human-readable reason for the `live` value |
| `modifiedExplanation` | string | Human-readable reason for the `modified` value |
| `dataExtract` | string | Raw OCR text |
| `faceData` | string | Face attributes, when `type` is `face` |
| `formRecognizer` | string | `key: value confidence: n` lines, when a document model ran |

An image that throws during processing is logged and omitted from the array —
a partially successful batch still returns `200`.

### Errors

| Status | Condition | Body |
|---|---|---|
| `400` | Neither `images` nor `paths` supplied, or a malformed URI | `ErrorResponse` |
| `503` | An upstream HTTP call failed | `ErrorResponse` |
| `500` | Any other unhandled exception | `ErrorResponse` |

```json
{ "errorMessage": "Invalid argument: No images or paths provided. Please provide image files or paths to images." }
```

---

## Examples

### Passport, uploaded as a file

```bash
curl -k -X POST https://localhost:5001/api/ImageAnalysis/imageAnalyse \
  -F "type=idpassport" \
  -F "gps=40.712776,-74.005974" \
  -F "clientIP=8.8.8.8" \
  -F "images=@passport.jpg"
```

```json
[
  {
    "photoName": "passport.jpg",
    "cameraMake": "Apple",
    "cameraModel": "iPhone 14 Pro",
    "gpsLocMeta": "40.712801,-74.006012",
    "gpsNow": "40.712776,-74.005974",
    "genuine": 100,
    "genuineExplanation": "100%. Camera maker and camera model have been recognized.",
    "live": 100,
    "liveExplanation": "100%. The location is within range and the photo is recent.",
    "modified": 100,
    "modifiedExplanation": "100%. An editing program and an editing time have been detected.",
    "formRecognizer": "DocumentNumber: X1234567 confidence: 0.97\nFirstName: ADA confidence: 0.99\n"
  }
]
```

> The `Modified` score reads counter-intuitively: **100 means no evidence of
> editing was found.** See [SCORING.md](SCORING.md#a-note-on-the-modified-score).

### Selfie, by URL

```bash
curl -k -X POST https://localhost:5001/api/ImageAnalysis/imageAnalyse \
  -F "type=face" \
  -F "paths=https://example.com/selfie.jpg"
```

### Invoice batch

```bash
curl -k -X POST https://localhost:5001/api/ImageAnalysis/imageAnalyse \
  -F "type=invoice" \
  -F "images=@invoice-01.pdf" \
  -F "images=@invoice-02.pdf"
```
