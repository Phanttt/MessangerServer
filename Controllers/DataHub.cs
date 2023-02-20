
using MessangerServer.Models;
using Microsoft.AspNetCore.SignalR;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;

namespace MessangerServer.Controllers
{
    public class DataHub : Hub
    {
        private MessangerContext context;
        public DataHub(MessangerContext context)
        {
            this.context = context;
            ContextInitializer.Init(this.context);
        }
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the group {groupName}.");
        }
        public async Task GetChats(int currUserId)
        {
            var currUser = await context.Users.Where(e => e.Id == currUserId).FirstOrDefaultAsync();
            var chats = await context.Chats.Include(e => e.Users).Where(e => e.Users.Contains(currUser)).ToListAsync();
            await Clients.All.SendAsync("GetChats", chats);
        }
        public async Task GetCurrentChat(int currchatId)
        {
            var currchat = await context.Chats.Include(e=>e.Users).Include(e=>e.Messages).FirstOrDefaultAsync(e=>e.Id == currchatId);
            await Clients.All.SendAsync("CurrentChat", currchat);
        }   
    }
}
