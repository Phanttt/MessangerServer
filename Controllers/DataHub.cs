using Microsoft.AspNetCore.SignalR;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;
using MessangerServer.Models.FromClient;
using MessangerServer.Models;

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
        public override async Task OnConnectedAsync()
        {
            var userid = Context.GetHttpContext().Request.Query["userid"];
            var chats = await context.AttachmentUserChats.Where(x => x.UserId == Convert.ToInt32(userid)).ToListAsync();

            foreach (var item in chats)
            {
                Console.WriteLine("User" + userid + ": +" + "Group" + item.ChatId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "Group"+item.ChatId);
            }
            
            await base.OnConnectedAsync();
        }
        public async Task SendMessage(ChatMessage message)
        {


            await Clients.Group("Group" + message.ChatId).SendAsync("ReceiveMessage", message);
        }
        public async Task GetChats(string currUserId)
        {
            var currUser = await context.Users.Where(e => e.Id == Convert.ToInt32(currUserId)).FirstOrDefaultAsync();
            var chats = await context.Chats.Include(e => e.Users).Where(e => e.Users.Contains(currUser)).ToListAsync();
            await Clients.Caller.SendAsync("GetChats", chats);
        }
        public async Task GetCurrentChat(int currchatId)
        {
            var currchat = await context.Chats.Include(e => e.Users).Include(e => e.Messages).FirstOrDefaultAsync(e => e.Id == currchatId);
            await Clients.Caller.SendAsync("CurrentChat", currchat);
        }


    } 
}
