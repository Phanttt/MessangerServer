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
            var userid = Convert.ToInt32(Context.GetHttpContext().Request.Query["userid"]);
            var chats = await context.AttachmentUserChats.Where(x => x.UserId == userid).ToListAsync();

            foreach (var item in chats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Group" + item.ChatId);
            }

            var user = await context.Users.FirstOrDefaultAsync(e => e.Id == userid);
            user.Status = true;
            await context.SaveChangesAsync();

            await base.OnConnectedAsync();
        }
        public async Task SendMessage(ChatMessage message)
        {
            var user = await context.Users.FirstOrDefaultAsync(e => e.Login == message.Sender.Login);
            Message newm = new Message()
            {
                Content = message.Content,
                Sender = user,
                ChatId = message.ChatId,
                IsReaded = message.IsReaded,
                DispatchTime = DateTime.Now
            };
            context.Add(newm);

            var curchat = await context.Chats.FirstOrDefaultAsync(e => e.Id == message.ChatId);
            curchat.Messages.Add(newm);
            await context.SaveChangesAsync();

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
            var messages = await context.Messages.Where(e => e.ChatId == currchatId).Include(e => e.Sender).OrderByDescending(e => e.DispatchTime).ToListAsync();
            await Clients.Caller.SendAsync("CurrentChatMessages", messages);
        }

        public async Task searchResult(string str, string id)
        {
            if (!string.IsNullOrEmpty(str))
            {

                var foundUsers = await context.Users.Where(e => e.Login.StartsWith(str)).ToListAsync();
                await Clients.Caller.SendAsync("SearchResult", foundUsers);
            }
            else
            {
                await Clients.Caller.SendAsync("SearchResult", null);
            }
        }
        public async Task GetAccountImage(string id) //ChangedData changedData, 
        {
            User user = await context.Users.FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(id));

            await Clients.Caller.SendAsync("Image", user.Avatar);
        }
     
    }
}
