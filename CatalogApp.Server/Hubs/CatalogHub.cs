using Microsoft.AspNetCore.SignalR;

namespace CatalogApp.Server.Hubs
{
    public class CatalogHub : Hub
    {
        // Клиенты подписываются автоматически при подключении.
        // Можно добавить методы для групп/аутентификации при необходимости.
        public override Task OnConnectedAsync()
        {
            // Можно логировать подключение: Context.ConnectionId
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Логирование отключения
            return base.OnDisconnectedAsync(exception);
        }
    }
}
