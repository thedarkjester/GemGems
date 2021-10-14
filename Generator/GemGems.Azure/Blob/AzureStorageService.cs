using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GemGems.App.Generator;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace GemGems.Azure.Blob
{
    public class AzureStorageService : IStorageService
    {
        private readonly string _storageConnectionString;
        public AzureStorageService(IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetConnectionString("AzureStorage");
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string container, string contentType)
        {
            BlobClient blob = await GetBlobClient(fileName, container);

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blob.Uri.ToString();
        }

        public async Task<string> UploadAsync(byte[] fileStream, string fileName, string container, string contentType)
        {
            BlobClient blob = await GetBlobClient(fileName, container);

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            using var stream = new MemoryStream(fileStream, writable: false);
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

            return blob.Uri.ToString();
        }

        private async Task<BlobClient> GetBlobClient(string fileName, string container)
        {
            var containerClient = new BlobContainerClient(_storageConnectionString, container);
            var createResponse = await containerClient.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blob = containerClient.GetBlobClient(fileName);
            return blob;
        }

        public async Task<Stream> GetFileUriAsync(string fileName, string container)
        {
            var containerClient = new BlobContainerClient(_storageConnectionString, container);
            var createResponse = await containerClient.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blob = containerClient.GetBlobClient(fileName);
            var content = await blob.DownloadContentAsync();

            return content.Value.Content.ToStream();
        }
    }
}
