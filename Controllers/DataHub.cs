using MessangerServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace MessangerServer.Controllers
{
    public class DataHub : Hub
    {
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
