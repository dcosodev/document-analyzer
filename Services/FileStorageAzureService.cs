using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public class FileStorageAzureService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "validator";

        public FileStorageAzureService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> StoreFileAzure(FileInfo file)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            BlobClient blobClient = containerClient.GetBlobClient(file.Name);

            using (FileStream uploadFileStream = File.OpenRead(file.FullName))
            {
                await blobClient.UploadAsync(uploadFileStream, true);
            }

            return file.Name;
        }

        public string GetBlobUrl(string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }

        public string GenerateSasUrl(string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public async Task DeleteAzureFile(string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
