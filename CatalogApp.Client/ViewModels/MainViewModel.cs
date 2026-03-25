using CatalogApp.Client.Services;
using CatalogApp.Shared.Dto;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CatalogApp.Client.ViewModels
{
    public class MainViewModel : ViewModelBase, IAsyncDisposable
    {
        private readonly IApiService _api;
        private readonly ISignalRService _signalR;
        private readonly Dispatcher _dispatcher;
        private readonly IDialogService _dialogService;
        private readonly ILogger<MainViewModel>? _logger;
        private readonly CancellationTokenSource _cts = new();

        public ObservableCollection<ProductViewModel> Products { get; } = new();
        public ICommand RefreshCommand { get; }
        public ICommand AddProductCommand { get; }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private int _handlersRegistered;

        public MainViewModel(
            IApiService api,
            ISignalRService signalR,
            IDialogService dialogService,
            ILogger<MainViewModel>? logger = null)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _signalR = signalR ?? throw new ArgumentNullException(nameof(signalR));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger;
            _dispatcher = Application.Current.Dispatcher;

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddProductCommand = new RelayCommand(OpenAddProduct);

            // Регистрируем обработчики ДО старта соединения — SignalRService буферизует их
            RegisterSignalRHandlers();

            // Запускаем инициализацию (загрузка + старт SignalR)
            _ = InitializeAsync(_cts.Token);
        }
        private async Task ExecuteSafeAsync(Func<Task> action)
        {
            try
            {
                ErrorMessage = "";
                await action().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // отмена — игнорируем или логируем при необходимости
            }
            catch (Exception ex)
            {
                // Показываем пользователю короткое сообщение
                ErrorMessage = ex.Message;
                // Для отладки можно логировать полную трассировку через Debug.WriteLine
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        private void RegisterSignalRHandlers()
        {
            if (Interlocked.CompareExchange(ref _handlersRegistered, 1, 0) == 1) return;

            _signalR.OnProductAdded(dto =>
            {
                _ = ExecuteSafeAsync(async () =>
                {
                    if (dto == null) throw new Exception("ProductAdded: получен null dto");

                    _dispatcher.Invoke(() =>
                    {
                        var exists = Products.FirstOrDefault(p => p.Id == dto.Id);
                        if (exists == null)
                        {
                            var vm = new ProductViewModel(dto, _api);
                            Products.Insert(0, vm);
                            _ = vm.LoadImageAsync(); // <- важный вызов
                        }
                        else
                        {
                            exists.UpdateFromDto(dto);
                            _ = exists.LoadImageAsync(); // обновляем картинку, если изменился источник
                        }
                    });
                });
            });

            _signalR.OnProductUpdated(dto =>
            {
                _ = HandleProductUpdated(dto);
            });

            _signalR.OnProductDeleted(id =>
            {
                _ = HandleProductDeleted(id);
            });

            _logger?.LogDebug("MainViewModel registered SignalR handlers (buffered)");
        }

        private async Task HandleProductAdded(ProductDto dto)
        {
            if (dto == null) return;
            await Task.Yield();
            _dispatcher.Invoke(() =>
            {
                var exists = Products.FirstOrDefault(p => p.Id == dto.Id);
                if (exists == null)
                    Products.Insert(0, new ProductViewModel(dto, _api));
                else
                    exists.UpdateFromDto(dto);
            });
        }

        private async Task HandleProductUpdated(ProductDto dto)
        {
            if (dto == null) return;
            await Task.Yield();
            _dispatcher.Invoke(() =>
            {
                var existing = Products.FirstOrDefault(p => p.Id == dto.Id);
                if (existing != null)
                    existing.UpdateFromDto(dto);
                else
                    Products.Insert(0, new ProductViewModel(dto, _api));
            });
        }

        private async Task HandleProductDeleted(int id)
        {
            await Task.Yield();
            _dispatcher.Invoke(() =>
            {
                var existing = Products.FirstOrDefault(p => p.Id == id);
                if (existing != null) Products.Remove(existing);
            });
        }

        private async Task InitializeAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                await LoadAsync().ConfigureAwait(false);

                var token = Application.Current.Properties["jwt"] as string;
                try
                {
                    await _signalR.StartAsync(token).ConfigureAwait(false);
                    _logger?.LogInformation("SignalR started from MainViewModel");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "SignalR StartAsync failed in MainViewModel");
                    ErrorMessage = "Не удалось подключиться к уведомлениям: " + ex.Message;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Initialization failed");
                ErrorMessage = ex.Message;
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                var token = Application.Current.Properties["jwt"] as string;
                var list = await _api.GetProductsAsync(token).ConfigureAwait(false);

                _dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var dto in list)
                    {
                        var vm = new ProductViewModel(dto, _api);
                        Products.Add(vm);
                        _ = vm.LoadImageAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load products");
                ErrorMessage = "Ошибка загрузки продуктов: " + ex.Message;
            }
        }

        private async void OpenAddProduct()
        {
            try
            {
                var result = _dialogService.ShowAddProductDialog();
                if (result) await LoadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening AddProduct dialog");
                ErrorMessage = "Ошибка при открытии окна добавления: " + ex.Message;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            try { await _signalR.StopAsync().ConfigureAwait(false); } catch (Exception ex) { _logger?.LogWarning(ex, "StopAsync failed"); }
            _cts.Dispose();
        }
    }
}
