using CatalogApp.Client.Services;
using CatalogApp.Shared.Api;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CatalogApp.Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IApiService _api;
        private string _username = "";
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(CanLogin));
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(CanLogin));
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
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly IDialogService _dialogService;
        public bool CanLogin =>
    !string.IsNullOrWhiteSpace(Username) &&
    !string.IsNullOrWhiteSpace(Password);
        public LoginViewModel(IApiService api, IDialogService dialogService)
        {
            _api = api;
            _dialogService = dialogService;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        }

        private async Task LoginAsync()
        {
            ErrorMessage = "";

            try
            {
                var token = await _api.LoginAsync(new LoginRequest { Username = Username, Password = Password });
                if (token == null)
                {
                    ErrorMessage = "Неверное имя пользователя или пароль.";
                    return;
                }

                Application.Current.Properties["jwt"] = token;
                _dialogService.ShowMainWindow();
            }
            catch (Exception ex)
            {
                // Показываем сообщение об ошибке на форме
                ErrorMessage = "Ошибка при подключении: " + ex.Message;
            }
        }

        private async Task RegisterAsync()
        {
            _dialogService.ShowRegistrationWindow();
        }
    }
}
