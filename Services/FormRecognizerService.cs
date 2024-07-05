using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public class FormRecognizerService
    {
        private readonly DocumentAnalysisClient _client;
        private readonly ILogger<FormRecognizerService> _logger;

        public FormRecognizerService(IConfiguration configuration, ILogger<FormRecognizerService> logger)
        {
            var endpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
            var apiKey = configuration["Azure:DocumentIntelligence:Key"];
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("Endpoint and Key must be provided");
            }

            _client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _logger = logger;
        }

        public async Task<string> AnalyzeDocumentAsync(string documentUri, string type)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(documentUri, UriKind.Absolute))
                {
                    throw new ArgumentException("The document URI must be an absolute URI.", nameof(documentUri));
                }

                if (string.IsNullOrEmpty(type))
                {
                    throw new ArgumentException("Type required", nameof(type));
                }

                AnalyzeDocumentOperation operation;
                if (type.Equals("idpassport", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("idback", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("idfront", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("passport", StringComparison.OrdinalIgnoreCase))
                {
                    operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-idDocument", new Uri(documentUri));
                }
                else if (type.Equals("invoice", StringComparison.OrdinalIgnoreCase))
                {
                    operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", new Uri(documentUri));
                }
                else
                {
                    return "Unrecognized type";
                }

                var analyzeResult = await operation.WaitForCompletionAsync();

                if (analyzeResult.Value.Documents == null || analyzeResult.Value.Documents.Count == 0)
                {
                    return "No documents were recognized in the image.";
                }

                var result = new StringBuilder();

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
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while analyzing the document");
                return $"An error occurred while analyzing the document: {e.Message}";
            }
        }
    }
}
