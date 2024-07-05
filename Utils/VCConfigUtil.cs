using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace ImageAnalysisAPI.Utils
{
    public static class VCConfigUtil
    {
        public static DocumentAnalysisClient GetDocumentAnalysisClient()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string endpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
            string apiKey = configuration["Azure:DocumentIntelligence:Key"];

            return new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public static IFaceClient GetFaceClient()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string endpoint = configuration["Azure:FaceAPI:Endpoint"];
            string apiKey = configuration["Azure:FaceAPI:SubscriptionKey"];

            return new FaceClient(new ApiKeyServiceClientCredentials(apiKey)) { Endpoint = endpoint };
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
