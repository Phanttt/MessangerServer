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
            var user = await context.Users.FirstOrDefaultAsync(e=>e.Login == message.Sender.Login);
            Message newm = new Message()
            {
                Content = message.Content,
                Sender = user,
                ChatId = message.ChatId,
                IsReaded = message.IsReaded,
                DispatchTime = DateTime.Now
            };
            context.Add(newm);

            var curchat = await context.Chats.FirstOrDefaultAsync(e=>e.Id == message.ChatId);
            curchat.Messages.Add(newm);
            await context.SaveChangesAsync();

            newm = await context.Messages.OrderByDescending(e=>e.DispatchTime).FirstOrDefaultAsync(e=>e.ChatId == message.ChatId);

            await Clients.Group("Group" + message.ChatId).SendAsync("ReceiveMessage", newm);
        }
        public async Task GetChats(string currUserId)
        {
            var currUser = await context.Users.Where(e => e.Id == Convert.ToInt32(currUserId)).FirstOrDefaultAsync();
            var chats = await context.Chats.Include(e => e.Users).Where(e => e.Users.Contains(currUser)).ToListAsync();
            foreach (var item in chats)
            {
                await context.Messages.Where(e => e.ChatId == item.Id).OrderByDescending(e => e.DispatchTime).FirstOrDefaultAsync();
            }
            await Clients.Caller.SendAsync("GetChats", chats);
        }
        public async Task getChatMessages(int currchatId)
        {
            //var currchat = await context.Chats.Include(e => e.Users).Include(e => e.Messages).FirstOrDefaultAsync(e => e.Id == currchatId);
            var messages = await context.Messages.Where(e => e.ChatId == currchatId).Include(e=>e.Sender).OrderByDescending(e=>e.DispatchTime).ToListAsync();
            await Clients.Caller.SendAsync("CurrentChatMessages", messages);
        }


    } 
}
