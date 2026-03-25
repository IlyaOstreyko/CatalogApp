using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Entities
{
    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        // BLOB и метаданные
        public byte[]? ImageData { get; set; }
        public string? ImageContentType { get; set; }
        public string? ImageFileName { get; set; }
        public int CategoryId { get; set; }
    }
}
