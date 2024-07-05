using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ImageAnalysisAPI.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlobServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            services.AddSingleton(blobServiceClient);
            return services;
        }
    }
}
