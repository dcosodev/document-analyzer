using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageAnalysisAPI.Utils
{
    public static class BlobStorageConfig
    {
        public static void AddBlobServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            services.AddSingleton(new BlobServiceClient(connectionString));
        }
    }
}
