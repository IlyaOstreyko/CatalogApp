using CatalogApp.Client.Services;
using CatalogApp.Client.Views;
using CatalogApp.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace CatalogApp.Client
{
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, cfg) =>
                {
                    cfg.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var baseUrl = context.Configuration["Api:BaseUrl"];

                    // Core services
                    services.AddSingleton<IApiService>(sp => new ApiService(baseUrl));
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<ISignalRService, SignalRService>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<SignalRService>>();
                        return new SignalRService(baseUrl, logger);
                    });

                    // File dialog service (if you have one)
                    services.AddTransient<IFileDialogService, FileDialogService>();

                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<AddProductViewModel>();
                    services.AddTransient<RegistrationViewModel>();

                    // Windows (registered so DI can construct them with VMs)
                    services.AddTransient<MainWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<AddProductWindow>();
                    services.AddTransient<RegistrationWindow>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            await _host.StartAsync();

            // Show login window (resolve via DI)
            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }

        // Удобный доступ к сервисам из кода (опционально)
        public IServiceProvider Services => _host!.Services;
    }
}
