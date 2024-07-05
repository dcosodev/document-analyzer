using System;
using System.IO;
using System.Threading.Tasks;
using ImageAnalysisAPI.Models;
using ImageAnalysisAPI.Utils;
using Microsoft.Extensions.Configuration;

namespace ImageAnalysisAPI.Services
{
    public class PhotoResponsesService
    {
        private readonly IConfiguration _configuration;
        private readonly OCRDataExtractService _ocrDataExtractService;
        private readonly GenuineCheckService _genuineCheckService;
        private readonly LiveCheckService _liveCheckService;
        private readonly ModifiedCheckService _modifiedCheckService;

        public PhotoResponsesService(IConfiguration configuration, OCRDataExtractService ocrDataExtractService,
            GenuineCheckService genuineCheckService, LiveCheckService liveCheckService,
            ModifiedCheckService modifiedCheckService)
        {
            _configuration = configuration;
            _ocrDataExtractService = ocrDataExtractService;
            _genuineCheckService = genuineCheckService;
            _liveCheckService = liveCheckService;
            _modifiedCheckService = modifiedCheckService;
        }

        public async Task<PhotoResponse> CreateResponseAsync(string localFilePath, string gpsNow, string clientIP, string type)
        {
            var fileUri = new Uri(Path.GetFullPath(localFilePath)).AbsoluteUri;

            var dataExtract = await _ocrDataExtractService.ExtractTextFromImageAsync(fileUri, type);
            var metadata = MetadataReaderUtil.ReadMetadata(new FileInfo(localFilePath));
            var geoDetails = await IPGeolocationUtil.GetGeoDetailsAsync(clientIP, _configuration["IPGeolocation:ApiKey"], _configuration["IPGeolocation:Endpoint"]);

            var genuineScore = _genuineCheckService.CheckIfGenuine(metadata.CameraMake, metadata.CameraModel);
            var liveScore = _liveCheckService.CheckIfLive(metadata.GpsDateTime, gpsNow, metadata.GpsLocMeta, geoDetails);
            var modifiedScore = _modifiedCheckService.CheckIfModified(metadata.EditingSoftware, metadata.DatetimeOriginal, metadata.DatetimeModified);

            var response = new PhotoResponse
            {
                PhotoName = Path.GetFileName(localFilePath),
                DataExtract = dataExtract,
                Genuine = genuineScore,
                Live = liveScore,
                Modified = modifiedScore,
                CameraMake = metadata.CameraMake,
                CameraModel = metadata.CameraModel,
                GpsLocMeta = metadata.GpsLocMeta,
                GpsNow = gpsNow,
                ClientIP = clientIP,
                GpsDateTime = metadata.GpsDateTime,
                DatetimeOriginal = metadata.DatetimeOriginal,
                DatetimeModified = metadata.DatetimeModified,
                EditingSoftware = metadata.EditingSoftware,
                LocationDetails = metadata.LocationDetails,
                CountryCode = metadata.CountryCode,
                GenuineExplanation = GenerateGenuineExplanation(genuineScore),
                LiveExplanation = GenerateLiveExplanation(liveScore),
                ModifiedExplanation = GenerateModifiedExplanation(modifiedScore)
            };

            return response;
        }

        private string GenerateGenuineExplanation(double genuine)
        {
            switch ((int)Math.Round(genuine))
            {
                case 100: return "100%. Camera maker and camera model have been recognized.";
                case 50: return "50%. Camera maker or camera model is not recognized.";
                case 0: return "0%. Neither camera maker nor camera model is recognized.";
                default: return "";
            }
        }

        private string GenerateLiveExplanation(double live)
        {
            switch ((int)Math.Round(live))
            {
                case 100: return "100%. The location is within range and the photo is recent.";
                case 75: return "75%. The IP location or GPS location is not within range. The photo is recent.";
                case 50: return "50%. The location is not within range or the photo was taken more than an hour ago.";
                case 25: return "25%. The IP location or GPS location is not within range. The photo was taken more than an hour ago.";
                case 0: return "0%. No location has been provided or it is not within range. The photo was taken more than an hour ago.";
                default: return "";
            }
        }

        private string GenerateModifiedExplanation(double modified)
        {
            switch ((int)Math.Round(modified))
            {
                case 100: return "100%. An editing program and an editing time have been detected.";
                case 50: return "50%. An editing program or an editing time has been detected.";
                case 0: return "0%. No editing program has been recognized nor has an editing time been detected.";
                default: return "";
            }
        }
    }
}
