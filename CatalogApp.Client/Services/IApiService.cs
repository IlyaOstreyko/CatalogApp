using CatalogApp.Shared.Api;
using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CatalogApp.Client.Services
{
    public interface IApiService
    {
        Task<string?> LoginAsync(LoginRequest request);
        Task RegisterAsync(RegisterRequest request);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckUsernameExistsAsync(string username);
        Task<IEnumerable<ProductDto>> GetProductsAsync(string? token = null);
        Task<ProductDto?> CreateProductAsync(ProductDto product, string? token = null);
        Task<string?> UploadFileAsync(string filePath, string? token = null);
        Task<ProductDto?> CreateProductWithImageAsync(ProductDto product, string? filePath, string? token = null);
        Task<Stream?> GetProductImageAsync(int productId);
    }
}
