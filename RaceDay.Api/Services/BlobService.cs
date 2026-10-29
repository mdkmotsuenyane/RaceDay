using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
            _connectionString = config["AzureStorage:ConnectionString"] ?? "";
            _containerName = config["AzureStorage:ContainerName"] ?? "event-images";
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);

            // 1. Check if we even have a real connection string
            if (string.IsNullOrEmpty(_connectionString) || _connectionString.Contains("YOUR_AZURE_CONNECTION_STRING_HERE"))
            {
                return await SaveLocallyAsync(fileStream, uniqueFileName);
            }

            // 2. Try to upload to Azure
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);
                await blobClient.UploadAsync(fileStream, true);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // If Azure fails for ANY reason, catch it, log it, and fall back to local storage
                Console.WriteLine($"Azure Upload Failed: {ex.Message}");
                Console.WriteLine("Falling back to local storage...");
                return await SaveLocallyAsync(fileStream, uniqueFileName);
            }
        }

        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl)) return;

            if (blobUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", blobUrl.TrimStart('/'));
                if (File.Exists(filePath)) File.Delete(filePath);
                return;
            }

            try
            {
                var uri = new Uri(blobUrl);
                string blobName = Path.GetFileName(uri.LocalPath);

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Ignore Azure delete errors so the app doesn't crash
            }
        }

        // Helper method for local fallback
        private async Task<string> SaveLocallyAsync(Stream fileStream, string uniqueFileName)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                // We need to reset the stream position in case Azure read it partially
                if (fileStream.CanSeek) fileStream.Position = 0;
                await fileStream.CopyToAsync(fileStreamOut);
            }

            return $"/uploads/{uniqueFileName}";
        }
    }
}
