using CatalogApp.Client.Services;
using CatalogApp.Shared.Api;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CatalogApp.Client.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly IApiService _api;
        private readonly IDialogService _dialogs;
        private string _username = "";
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool CanRegister =>
    !string.IsNullOrWhiteSpace(Username) &&
    !string.IsNullOrWhiteSpace(Email) &&
    !string.IsNullOrWhiteSpace(Password) &&
    !string.IsNullOrWhiteSpace(ConfirmPassword);

        public ICommand BackCommand { get; }
        public ICommand RegisterCommand { get; }

        public RegistrationViewModel(IApiService api, IDialogService dialogs)
        {
            _api = api;
            _dialogs = dialogs;

            BackCommand = new RelayCommand(Back);
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        }

        private void Back()
        {
            _dialogs.ShowLoginWindow();
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = "";
            // 1. Проверка пустых полей
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Все поля должны быть заполнены";
                return;
            }

            // 2. Проверка имени
            if (!Regex.IsMatch(Username, @"^[A-Za-zА-Яа-яЁё\s\-]+$"))
            {
                ErrorMessage = "Имя может содержать только буквы, пробелы и тире";
                return;
            }

            // 3. Проверка совпадения паролей
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            bool existsUsername = await _api.CheckUsernameExistsAsync(Username);
            if (existsUsername)
            {
                ErrorMessage = "Этот Username уже зарегистрирован";
                return;
            }
            // 4. Проверка email в базе
            bool exists = await _api.CheckEmailExistsAsync(Email);
            if (exists)
            {
                ErrorMessage = "Этот email уже зарегистрирован";
                return;
            }

            // Регистрация
            await _api.RegisterAsync(new RegisterRequest
            {
                Username = Username,
                Email = Email,
                Password = Password
            });
            //ErrorMessage = "Регистрация успешна";
            _dialogs.ShowLoginWindow();
        }
    }
}
