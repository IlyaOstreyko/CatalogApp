using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Shared.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        // Флаг, указывающий, есть ли изображение
        public bool HasImage { get; set; } = false;
        public int CategoryId { get; set; }
    }
}
