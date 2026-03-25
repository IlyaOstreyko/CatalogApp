using System;
using System.Collections.Generic;
using System.Text;
using CatalogApp.Shared.Dto;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(ProductDto dto);
        Task<int> CreateWithImageAsync(ProductDto dto, byte[]? imageData, string? contentType, string? fileName);
        Task UpdateAsync(ProductDto dto);
        Task UpdateImageAsync(int productId, byte[] imageData, string contentType, string fileName);
        Task DeleteAsync(int id);
        Task<(byte[] Data, string ContentType)?> GetImageAsync(int productId);
    }
}
