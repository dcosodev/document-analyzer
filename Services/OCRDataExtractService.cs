using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public class OCRDataExtractService
    {
        private readonly DocumentAnalysisClient _client;

        public OCRDataExtractService(IConfiguration configuration)
        {
            var endpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
            var apiKey = configuration["Azure:DocumentIntelligence:Key"];
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("Endpoint and Key must be provided");
            }

            _client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<string> ExtractTextFromImageAsync(string imageUrl, string type)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                throw new ArgumentException("The image URL must be an absolute URI.", nameof(imageUrl));
            }

            AnalyzeDocumentOperation operation;
            try
            {
                if (type.Equals("idpassport", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("idback", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("idfront", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("passport", StringComparison.OrdinalIgnoreCase))
                {
                    operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-idDocument", new Uri(imageUrl));
                }
                else if (type.Equals("invoice", StringComparison.OrdinalIgnoreCase))
                {
                    operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", new Uri(imageUrl));
                }
                else
                {
                    return "Unrecognized type";
                }
            }
            catch (Exception e)
            {
                return $"An error occurred while analyzing the image: {e.Message}";
            }

            var analyzeResult = await operation.WaitForCompletionAsync();

            if (analyzeResult.Value.Documents == null || analyzeResult.Value.Documents.Count == 0)
            {
                return "No documents were recognized in the image.";
            }

            var result = new System.Text.StringBuilder();

            foreach (var analyzedDocument in analyzeResult.Value.Documents)
            {
                foreach (var field in analyzedDocument.Fields)
                {
                    if (field.Value.FieldType == DocumentFieldType.String)
                    {
                        result.AppendLine($"{field.Key}: {field.Value.Value.AsString()} confidence: {field.Value.Confidence}");
                    }
                    else if (field.Value.FieldType == DocumentFieldType.Date)
                    {
                        result.AppendLine($"{field.Key}: {field.Value.Value.AsDate()} confidence: {field.Value.Confidence}");
                    }
                    else if (field.Value.FieldType == DocumentFieldType.Double)
                    {
                        result.AppendLine($"{field.Key}: {field.Value.Value.AsDouble()} confidence: {field.Value.Confidence}");
                    }
                }
            }

            return result.ToString();
        }
    }
}
