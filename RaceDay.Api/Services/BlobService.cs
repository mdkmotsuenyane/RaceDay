using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RaceDay.Api.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(IConfiguration config)
        {
            _connectionString = config["AzureStorage:ConnectionString"]!;
            _containerName = config["AzureStorage:ContainerName"]!;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(fileStream, true);

            return blobClient.Uri.ToString();
        }

        // NEW METHOD FOR DELETING
        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl)) return;

            var uri = new Uri(blobUrl);
            string blobName = Path.GetFileName(uri.LocalPath);

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
