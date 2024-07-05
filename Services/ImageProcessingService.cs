using ImageAnalysisAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageAnalysisAPI.Utils;

namespace ImageAnalysisAPI.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly PhotoResponsesService _photoResponsesService;
        private readonly ValidationService _validationService;
        private readonly FileStorageLocalService _fileStorageLocalService;
        private readonly FileStorageAzureService _fileStorageAzureService;
        private readonly FaceRecognitionService _faceRecognitionService;
        private readonly FormRecognizerService _formRecognizerService;
        private readonly ILogger<ImageProcessingService> _logger;

        public ImageProcessingService(
            PhotoResponsesService photoResponsesService,
            ValidationService validationService,
            FileStorageLocalService fileStorageLocalService,
            FileStorageAzureService fileStorageAzureService,
            FaceRecognitionService faceRecognitionService,
            FormRecognizerService formRecognizerService,
            ILogger<ImageProcessingService> logger)
        {
            _photoResponsesService = photoResponsesService;
            _validationService = validationService;
            _fileStorageLocalService = fileStorageLocalService;
            _fileStorageAzureService = fileStorageAzureService;
            _faceRecognitionService = faceRecognitionService;
            _formRecognizerService = formRecognizerService;
            _logger = logger;
        }

        public async Task<List<PhotoResponse>> AnalyzeImages(string[] paths, string gps, IFormFile[] images, bool clientCamera, string clientIP, string type)
        {
            List<PhotoResponse> responses = new List<PhotoResponse>();

            if (!_validationService.ValidateInput(paths, images))
            {
                throw new ArgumentException("No images or paths provided. Please provide image files or paths to images.");
            }

            if (images != null && images.Length > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var localFile = await _fileStorageLocalService.StoreFileLocalAsync(image);
                        try
                        {
                            var metadata = MetadataReaderUtil.ReadMetadata(localFile);
                            var response = await _photoResponsesService.CreateResponseAsync(localFile.FullName, gps, clientIP, type);
                            response.CameraMake = metadata.CameraMake;
                            response.CameraModel = metadata.CameraModel;
                            response.GpsLocMeta = metadata.GpsLocMeta;
                            response.GpsNow = gps;
                            response.ClientIP = clientIP;
                            response.GpsDateTime = metadata.GpsDateTime;
                            response.DatetimeOriginal = metadata.DatetimeOriginal;
                            response.DatetimeModified = metadata.DatetimeModified;
                            response.EditingSoftware = metadata.EditingSoftware;
                            response.LocationDetails = metadata.LocationDetails;
                            response.CountryCode = metadata.CountryCode;
                            response.PhotoName = Path.GetFileName(localFile.FullName);

                            // Upload to Azure Storage and get URL
                            var blobName = await _fileStorageAzureService.StoreFileAzure(localFile);
                            var imageUrl = _fileStorageAzureService.GetBlobUrl(blobName);

                            _logger.LogInformation("Blob URL: {ImageUrl}", imageUrl); // Imprimir la URL del blob

                            _logger.LogInformation("Processing image for type: {Type}", type);

                            // Análisis de reconocimiento facial y documentos
                            if (type.Equals("face", StringComparison.OrdinalIgnoreCase))
                            {
                                var faceData = await _faceRecognitionService.FaceRecognitionDataAsync(imageUrl);
                                _logger.LogInformation("Face Data: {FaceData}", faceData);
                                if (string.IsNullOrEmpty(faceData) || faceData == "No faces detected.")
                                {
                                    _logger.LogWarning("No faces detected for image: {ImageUrl}", imageUrl);
                                }
                                else
                                {
                                    response.FaceData = faceData;
                                }
                            }
                            else
                            {
                                var formRecognizerData = await _formRecognizerService.AnalyzeDocumentAsync(imageUrl, type);
                                response.FormRecognizer = formRecognizerData;
                            }

                            responses.Add(response);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while processing the image");
                        }
                        finally
                        {
                            _fileStorageLocalService.DeleteLocalFile(localFile);
                        }
                    }
                }
            }

            if (paths != null && paths.Length > 0)
            {
                foreach (var path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                        {
                            var uri = new Uri(path);
                            var tempFilePath = Path.Combine(Path.GetTempPath(), $"downloaded-{Path.GetFileName(uri.LocalPath)}");
                            var localFile = new FileInfo(tempFilePath);

                            using (var client = new HttpClient())
                            {
                                var responseMessage = await client.GetAsync(uri);
                                responseMessage.EnsureSuccessStatusCode();

                                using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                                {
                                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                    {
                                        await stream.CopyToAsync(fileStream);
                                    }
                                }
                            }

                            try
                            {
                                var metadata = MetadataReaderUtil.ReadMetadata(localFile);
                                var response = await _photoResponsesService.CreateResponseAsync(localFile.FullName, gps, clientIP, type);
                                response.CameraMake = metadata.CameraMake;
                                response.CameraModel = metadata.CameraModel;
                                response.GpsLocMeta = metadata.GpsLocMeta;
                                response.GpsNow = gps;
                                response.ClientIP = clientIP;
                                response.GpsDateTime = metadata.GpsDateTime;
                                response.DatetimeOriginal = metadata.DatetimeOriginal;
                                response.DatetimeModified = metadata.DatetimeModified;
                                response.EditingSoftware = metadata.EditingSoftware;
                                response.LocationDetails = metadata.LocationDetails;
                                response.CountryCode = metadata.CountryCode;
                                response.PhotoName = Path.GetFileName(localFile.FullName);

                                // Upload to Azure Storage and get URL
                                var blobName = await _fileStorageAzureService.StoreFileAzure(localFile);
                                var imageUrl = _fileStorageAzureService.GetBlobUrl(blobName);

                                _logger.LogInformation("Blob URL: {ImageUrl}", imageUrl); // Imprimir la URL del blob

                                _logger.LogInformation("Processing image from URL for type: {Type}", type);

                                // Análisis de reconocimiento facial y documentos
                                if (type.Equals("face", StringComparison.OrdinalIgnoreCase))
                                {
                                    var faceData = await _faceRecognitionService.FaceRecognitionDataAsync(imageUrl);
                                    _logger.LogInformation("Face Data: {FaceData}", faceData);
                                    if (string.IsNullOrEmpty(faceData) || faceData == "No faces detected.")
                                    {
                                        _logger.LogWarning("No faces detected for image: {ImageUrl}", imageUrl);
                                    }
                                    else
                                    {
                                        response.FaceData = faceData;
                                    }
                                }
                                else
                                {
                                    var formRecognizerData = await _formRecognizerService.AnalyzeDocumentAsync(imageUrl, type);
                                    response.FormRecognizer = formRecognizerData;
                                }

                                responses.Add(response);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred while processing the image from URL");
                            }
                            finally
                            {
                                _fileStorageLocalService.DeleteLocalFile(localFile);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("The provided URI '{Path}' is not a well-formed absolute URI.", path);
                        }
                    }
                }
            }

            return responses;
        }
    }
}
