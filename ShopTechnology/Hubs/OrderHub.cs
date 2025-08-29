using Microsoft.AspNetCore.SignalR;
using ShopTechnology.Models;

namespace ShopTechnology.Hubs
{
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to OrderHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from OrderHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinOrderTracking(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
            _logger.LogInformation("Client joined order tracking group: {OrderId}", orderId);
        }

        public async Task LeaveOrderTracking(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
            _logger.LogInformation("Client left order tracking group: {OrderId}", orderId);
        }

        public async Task JoinUserOrders(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_orders_{userId}");
            _logger.LogInformation("User joined orders group: {UserId}", userId);
        }

        public async Task LeaveUserOrders(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_orders_{userId}");
            _logger.LogInformation("User left orders group: {UserId}", userId);
        }
    }
}
