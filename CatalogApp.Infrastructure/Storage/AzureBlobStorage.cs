using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CatalogApp.Infrastructure.Interfaces;

namespace CatalogApp.Infrastructure.Storage
{
    public class AzureBlobStorage 
    {
        private readonly BlobContainerClient _container;

        public AzureBlobStorage(string connectionString, string containerName)
        {
            var serviceClient = new BlobServiceClient(connectionString);
            _container = serviceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var blobClient = _container.GetBlobClient(fileName);
            var headers = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(stream, headers, cancellationToken: cancellationToken);
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadAsync(string blobUrl, CancellationToken cancellationToken = default)
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.DownloadAsync(cancellationToken);
            return response.Value.Content;
        }

        public async Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default)
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
