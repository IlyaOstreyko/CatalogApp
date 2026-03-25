using AutoMapper;
using CatalogApp.Infrastructure.Data;
using CatalogApp.Infrastructure.Entities;
using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Shared.Dto;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly IMapper _mapper;

        public ProductRepository(IDbConnection connection, IDbTransaction transaction, IMapper mapper)
        {
            _connection = connection;
            _transaction = transaction;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            const string sql = @"
            SELECT Id, Name, Description, Price,
                   CASE WHEN DATALENGTH(ImageData) IS NULL THEN 0 ELSE 1 END AS HasImage,
                   CategoryId
            FROM Products";
            var rows = await _connection.QueryAsync(sql, transaction: _transaction);
            return rows.Select(r => new ProductDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Price = r.Price,
                HasImage = r.HasImage == 1,
                CategoryId = r.CategoryId
            });
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            const string sql = @"
            SELECT Id, Name, Description, Price,
                   CASE WHEN DATALENGTH(ImageData) IS NULL THEN 0 ELSE 1 END AS HasImage,
                   CategoryId
            FROM Products WHERE Id = @Id";
            var row = await _connection.QueryFirstOrDefaultAsync(sql, new { Id = id }, _transaction);
            if (row == null) return null;
            return new ProductDto
            {
                Id = row.Id,
                Name = row.Name,
                Description = row.Description,
                Price = row.Price,
                HasImage = row.HasImage == 1,
                CategoryId = row.CategoryId
            };
        }

        public async Task<int> CreateAsync(ProductDto dto)
        {
            const string sql = @"
            INSERT INTO Products (Name, Description, Price, CategoryId)
            VALUES (@Name, @Description, @Price, @CategoryId);
            SELECT CAST(SCOPE_IDENTITY() as int);";
            return await _connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Description,
                dto.Price,
                dto.CategoryId
            }, _transaction);
        }

        public async Task<int> CreateWithImageAsync(ProductDto dto, byte[]? imageData, string? contentType, string? fileName)
        {
            const string sql = @"
            INSERT INTO Products (Name, Description, Price, ImageData, ImageContentType, ImageFileName, CategoryId)
            VALUES (@Name, @Description, @Price, @ImageData, @ImageContentType, @ImageFileName, @CategoryId);
            SELECT CAST(SCOPE_IDENTITY() as int);";
            return await _connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Description,
                dto.Price,
                ImageData = imageData,
                ImageContentType = contentType,
                ImageFileName = fileName,
                dto.CategoryId
            }, _transaction);
        }

        public async Task UpdateAsync(ProductDto dto)
        {
            const string sql = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                CategoryId = @CategoryId
            WHERE Id = @Id";
            await _connection.ExecuteAsync(sql, new
            {
                dto.Name,
                dto.Description,
                dto.Price,
                dto.CategoryId,
                dto.Id
            }, _transaction);
        }

        public async Task UpdateImageAsync(int productId, byte[] imageData, string contentType, string fileName)
        {
            const string sql = @"
            UPDATE Products
            SET ImageData = @ImageData,
                ImageContentType = @ImageContentType,
                ImageFileName = @ImageFileName
            WHERE Id = @Id";
            await _connection.ExecuteAsync(sql, new
            {
                ImageData = imageData,
                ImageContentType = contentType,
                ImageFileName = fileName,
                Id = productId
            }, _transaction);
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Products WHERE Id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = id }, _transaction);
        }

        public async Task<(byte[] Data, string ContentType)?> GetImageAsync(int productId)
        {
            const string sql = "SELECT ImageData, ImageContentType FROM Products WHERE Id = @Id";
            var row = await _connection.QueryFirstOrDefaultAsync(sql, new { Id = productId }, _transaction);
            if (row == null || row.ImageData == null) return null;
            return (row.ImageData as byte[], row.ImageContentType as string ?? "application/octet-stream");
        }
    }
}
