using CatalogApp.Shared.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Connections;

namespace CatalogApp.Client.Services
{
    public class SignalRService : ISignalRService, IAsyncDisposable
    {
        private readonly string _hubUrl;
        private readonly ILogger<SignalRService> _logger;
        private HubConnection? _connection;
        private bool _disposed;

        private readonly List<Action<ProductDto>> _productAddedHandlers = new();
        private readonly List<Action<ProductDto>> _productUpdatedHandlers = new();
        private readonly List<Action<int>> _productDeletedHandlers = new();
        private readonly object _sync = new();

        public SignalRService(string baseUrl, ILogger<SignalRService> logger)
        {
            _hubUrl = baseUrl.TrimEnd('/') + "/hubs/catalog";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(string? accessToken = null)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SignalRService));
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                _logger.LogDebug("SignalR already connected");
                return;
            }

            await CreateAndStartConnection(accessToken, HttpTransportType.None).ConfigureAwait(false);
        }

        private async Task CreateAndStartConnection(string? accessToken, HttpTransportType transports)
        {
            await SafeDisposeConnection().ConfigureAwait(false);

            _connection = BuildConnection(accessToken, transports);
            AttachLifecycleHandlers(_connection);

            try
            {
                _logger.LogInformation("Starting SignalR connection to {HubUrl} (transports: {Transports})", _hubUrl, transports);
                await _connection.StartAsync().ConfigureAwait(false);
                _logger.LogInformation("SignalR started. State: {State}", _connection.State);
                RegisterBufferedHandlers();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR start failed with transports {Transports}", transports);
                if (transports == HttpTransportType.None)
                {
                    _logger.LogInformation("Retrying SignalR start with LongPolling");
                    await CreateAndStartConnection(accessToken, HttpTransportType.LongPolling).ConfigureAwait(false);
                }
                else
                {
                    await SafeDisposeConnection().ConfigureAwait(false);
                    throw;
                }
            }
        }

        private HubConnection BuildConnection(string? accessToken, HttpTransportType transports)
        {
            var builder = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    if (transports != HttpTransportType.None)
                        options.Transports = transports;
                    if (!string.IsNullOrEmpty(accessToken))
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .WithAutomaticReconnect()
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddDebug();
                });

            return builder.Build();
        }

        private void AttachLifecycleHandlers(HubConnection connection)
        {
            connection.Reconnecting += error =>
            {
                _logger.LogWarning(error, "SignalR reconnecting");
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                _logger.LogInformation("SignalR reconnected. ConnectionId: {ConnectionId}", connectionId);
                try { RegisterBufferedHandlers(); } catch (Exception ex) { _logger.LogWarning(ex, "Re-register handlers failed"); }
                return Task.CompletedTask;
            };

            connection.Closed += async ex =>
            {
                _logger.LogWarning(ex, "SignalR closed");
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                if (_disposed) return;
                try
                {
                    _logger.LogInformation("Attempting to restart SignalR after close");
                    await connection.StartAsync().ConfigureAwait(false);
                }
                catch (Exception restartEx)
                {
                    _logger.LogError(restartEx, "Automatic restart failed; attempting LongPolling fallback");
                    try { await CreateAndStartConnection(null, HttpTransportType.LongPolling).ConfigureAwait(false); }
                    catch (Exception lpEx) { _logger.LogError(lpEx, "LongPolling fallback failed"); }
                }
            };
        }

        private void RegisterBufferedHandlers()
        {
            if (_connection == null) return;

            lock (_sync)
            {
                foreach (var h in _productAddedHandlers) _connection.On<ProductDto>("ProductAdded", h);
                foreach (var h in _productUpdatedHandlers) _connection.On<ProductDto>("ProductUpdated", h);
                foreach (var h in _productDeletedHandlers) _connection.On<int>("ProductDeleted", h);
            }

            _logger.LogDebug("Buffered SignalR handlers registered on connection");
        }

        private async Task SafeDisposeConnection()
        {
            if (_connection == null) return;
            try { await _connection.StopAsync().ConfigureAwait(false); } catch (Exception ex) { _logger.LogDebug(ex, "StopAsync failed"); }
            try { await _connection.DisposeAsync().ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogDebug(ex, "DisposeAsync failed"); }
            finally { _connection = null; }
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping SignalR connection");
            await SafeDisposeConnection().ConfigureAwait(false);
        }

        public void OnProductAdded(Action<ProductDto> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_sync)
            {
                _productAddedHandlers.Add(handler);
                if (_connection != null) _connection.On<ProductDto>("ProductAdded", handler);
            }
            _logger.LogDebug("OnProductAdded handler registered (buffered={Count})", _productAddedHandlers.Count);
        }

        public void OnProductUpdated(Action<ProductDto> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_sync)
            {
                _productUpdatedHandlers.Add(handler);
                if (_connection != null) _connection.On<ProductDto>("ProductUpdated", handler);
            }
            _logger.LogDebug("OnProductUpdated handler registered (buffered={Count})", _productUpdatedHandlers.Count);
        }

        public void OnProductDeleted(Action<int> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_sync)
            {
                _productDeletedHandlers.Add(handler);
                if (_connection != null) _connection.On<int>("ProductDeleted", handler);
            }
            _logger.LogDebug("OnProductDeleted handler registered (buffered={Count})", _productDeletedHandlers.Count);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            _logger.LogInformation("Disposing SignalRService");
            await SafeDisposeConnection().ConfigureAwait(false);
            lock (_sync)
            {
                _productAddedHandlers.Clear();
                _productUpdatedHandlers.Clear();
                _productDeletedHandlers.Clear();
            }
        }
    }
}
