using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public class FaceRecognitionService
    {
        private readonly string subscriptionKey;
        private readonly string endpoint;
        private readonly IFaceClient faceClient;
        private readonly ILogger<FaceRecognitionService> _logger;

        public FaceRecognitionService(IConfiguration configuration, ILogger<FaceRecognitionService> logger)
        {
            subscriptionKey = configuration["Azure:FaceAPI:SubscriptionKey"];
            endpoint = configuration["Azure:FaceAPI:Endpoint"];
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(subscriptionKey))
            {
                throw new ArgumentNullException("Endpoint and SubscriptionKey must be provided");
            }

            faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };
            _logger = logger;
        }

        public async Task<string> FaceRecognitionDataAsync(string imageUrl)
        {
            try
            {
                _logger.LogInformation("Starting face recognition for image: {ImageUrl}", imageUrl);

                var faceAttributes = new List<FaceAttributeType>
                {
                    FaceAttributeType.Age,
                    FaceAttributeType.Gender,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses,
                    FaceAttributeType.Hair,
                    FaceAttributeType.Occlusion,
                    FaceAttributeType.Blur,
                    FaceAttributeType.Exposure,
                    FaceAttributeType.Noise
                };

                var faces = await faceClient.Face.DetectWithUrlAsync(imageUrl, returnFaceAttributes: faceAttributes);

                if (faces == null || faces.Count == 0)
                {
                    _logger.LogWarning("No faces detected for image: {ImageUrl}", imageUrl);
                    return "No faces detected.";
                }

                var result = new StringBuilder();
                foreach (var face in faces)
                {
                    _logger.LogInformation("Face detected: {FaceId} with attributes", face.FaceId);
                    result.AppendLine($"Face detected with attributes:");
                    result.AppendLine($"- Age: {face.FaceAttributes.Age}");
                    result.AppendLine($"- Gender: {face.FaceAttributes.Gender}");
                    result.AppendLine($"- Smile: {face.FaceAttributes.Smile}");
                    result.AppendLine($"- Emotions: {string.Join(", ", face.FaceAttributes.Emotion.ToRankedList())}");
                    result.AppendLine($"- Glasses: {face.FaceAttributes.Glasses}");
                    result.AppendLine($"- Hair: {string.Join(", ", face.FaceAttributes.Hair.HairColor)}");
                    result.AppendLine($"- Occlusion: Eye - {face.FaceAttributes.Occlusion.EyeOccluded}, Mouth - {face.FaceAttributes.Occlusion.MouthOccluded}, Forehead - {face.FaceAttributes.Occlusion.ForeheadOccluded}");
                    result.AppendLine($"- Blur: {face.FaceAttributes.Blur.BlurLevel}, Value: {face.FaceAttributes.Blur.Value}");
                    result.AppendLine($"- Exposure: {face.FaceAttributes.Exposure.ExposureLevel}, Value: {face.FaceAttributes.Exposure.Value}");
                    result.AppendLine($"- Noise: {face.FaceAttributes.Noise.NoiseLevel}, Value: {face.FaceAttributes.Noise.Value}");
                }

                _logger.LogInformation("Face recognition completed for image: {ImageUrl}", imageUrl);
                return result.ToString();
            }
            catch (APIErrorException e)
            {
                _logger.LogError(e, "API error occurred while analyzing the face for image: {ImageUrl}", imageUrl);
                if (e.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Access to the Face API was forbidden. Please check your subscription key and endpoint.");
                }
                return $"An error occurred while analyzing the face: {e.Body?.Error?.Message ?? e.Message}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while analyzing the face for image: {ImageUrl}", imageUrl);
                return $"An error occurred while analyzing the face: {e.Message}";
            }
        }
    }

    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string _subscriptionKey;

        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            _subscriptionKey = subscriptionKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
