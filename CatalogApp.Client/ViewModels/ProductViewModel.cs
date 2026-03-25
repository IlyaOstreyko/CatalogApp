using CatalogApp.Client.Services;
using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace CatalogApp.Client.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IApiService _api;

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public int CategoryId { get; private set; }

        // Флаг из DTO
        public bool HasImage { get; private set; }

        // Картинка, которую будем показывать в UI
        private ImageSource? _imageSource;
        public ImageSource? ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public ProductViewModel(ProductDto dto, IApiService api)
        {
            _api = api;
            UpdateFromDto(dto);
        }

        public void UpdateFromDto(ProductDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Description = dto.Description;
            Price = dto.Price;
            CategoryId = dto.CategoryId;
            HasImage = dto.HasImage;

            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(CategoryId));
            OnPropertyChanged(nameof(HasImage));
        }

        public async Task LoadImageAsync()
        {
            if (!HasImage)
                return;

            try
            {
                // API должен уметь вернуть поток картинки по Id
                using var stream = await _api.GetProductImageAsync(Id);
                if (stream == null)
                    return;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = stream;
                bmp.EndInit();
                bmp.Freeze();

                ImageSource = bmp;
            }
            catch
            {
                // TODO: можно добавить placeholder
            }
        }
    }
}
