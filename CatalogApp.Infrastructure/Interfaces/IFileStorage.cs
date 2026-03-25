using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IFileStorage
    {
        Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task<Stream> DownloadAsync(string id, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
