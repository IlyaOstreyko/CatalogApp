using CatalogApp.Client.Services;
using CatalogApp.Shared.Dto;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CatalogApp.Client.ViewModels
{
    public class AddProductViewModel : ViewModelBase
    {
        private readonly IApiService _api;
        private readonly IFileDialogService _fileDialog;
        private static readonly Regex NameRegex = new(@"^[\p{L}\d\s\-]+$", RegexOptions.Compiled);
        private static readonly Regex PriceRegex = new(@"^\d+$", RegexOptions.Compiled);
        private string _name = "";
        private string _description = "";
        private string _price = "0";
        private string _nameError = "";
        private string _priceError = "";
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ValidateName();
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                {
                    ValidatePrice();
                }
            }
        }
        public string NameError
        {
            get => _nameError;
            private set => SetProperty(ref _nameError, value);
        }

        public string PriceError
        {
            get => _priceError;
            private set => SetProperty(ref _priceError, value);
        }
        public string? SelectedFile { get; set; }
        public ICommand ChooseFileCommand { get; }
        public ICommand UploadCreateCommand { get; }

        public AddProductViewModel(IApiService api, IFileDialogService fileDialog)
        {
            _api = api;
            _fileDialog = fileDialog;
            ChooseFileCommand = new RelayCommand(ChooseFile);
            UploadCreateCommand = new AsyncRelayCommand(UploadCreateAsync);
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                NameError = "Название обязательно.";
                return;
            }

            if (!NameRegex.IsMatch(Name))
            {
                NameError = "Название может содержать только буквы, цифры, пробелы и дефис.";
                return;
            }

            NameError = "";
        }

        private void ValidatePrice()
        {
            if (string.IsNullOrWhiteSpace(Price))
            {
                PriceError = "Цена обязательна.";
                return;
            }

            if (!PriceRegex.IsMatch(Price))
            {
                PriceError = "Цена должна содержать только цифры.";
                return;
            }

            PriceError = "";
        }

        private void ChooseFile()
        {
            SelectedFile = _fileDialog.OpenFileDialog("Images|*.png;*.jpg;*.jpeg;*.gif");
            OnPropertyChanged(nameof(SelectedFile));
        }

        private async Task UploadCreateAsync()
        {
            if (string.IsNullOrEmpty(SelectedFile))
            {
                MessageBox.Show("Choose image");
                return;
            }

            var token = Application.Current.Properties["jwt"] as string;

            var product = new ProductDto
            {
                Name = Name,
                Description = Description,
                Price = decimal.TryParse(Price, out var p) ? p : 0,
                CategoryId = 1
            };

            var created = await _api.CreateProductWithImageAsync(product, SelectedFile, token);
            if (created == null) { MessageBox.Show("Create failed"); return; }
            MessageBox.Show("Created");
            foreach (var w in Application.Current.Windows)
            {
                if (w is Views.AddProductWindow) { ((Window)w).Close(); break; }
            }
        }
    }
}
