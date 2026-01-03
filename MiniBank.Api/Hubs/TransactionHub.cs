using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace MiniBank.Api.Hubs
{
    [Authorize]
    public class TransactionHub : Hub
    {
        public async Task JoinAccountGroup(string accountNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountNumber}");
            await Clients.Caller.SendAsync("Joined", $"Joined account_{accountNumber}");
        }

        public async Task LeaveAccountGroup(string accountNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account_{accountNumber}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await Clients.Caller.SendAsync("Connected", $"Connected as {userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}