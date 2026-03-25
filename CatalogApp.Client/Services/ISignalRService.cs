using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public interface ISignalRService : IAsyncDisposable
    {
        Task StartAsync(string? accessToken = null);
        Task StopAsync();
        void OnProductAdded(Action<ProductDto> handler);
        void OnProductUpdated(Action<ProductDto> handler);
        void OnProductDeleted(Action<int> handler);
    }
}
