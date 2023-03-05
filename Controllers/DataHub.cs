using Microsoft.AspNetCore.SignalR;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;
using MessangerServer.Models.FromClient;
using MessangerServer.Models;
using MessangerServer.Models.ToClient;

namespace MessangerServer.Controllers
{
    public class DataHub : Hub
    {
        private MessangerContext context;
        public DataHub(MessangerContext context)
        {
            this.context = context;
            //ContextInitializer.Init(this.context);
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
            if (user!=null)
            {
                user.Status = true;
            }

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

            var chatsToSend = await context.AttachmentUserChats
                .Include(a => a.Chat)
                .ThenInclude(e=>e.Messages)
                .Include(a => a.User)
                .GroupBy(a => a.Chat)
                .Select(g => new ChatToSend
                {
                    Id = g.Key.Id,
                    Type = g.Key.Type,
                    Users = g.Select(a => a.User).ToList(),
                    Messages = g.Select(a => a.Chat.Messages).FirstOrDefault()
                })
                .ToListAsync();


            await Clients.Caller.SendAsync("GetChats", chatsToSend);
        }
        public async Task getChatMessages(int currchatId)
        {
            var messages = await context.Messages.Where(e => e.ChatId == currchatId).Include(e => e.Sender).OrderByDescending(e => e.DispatchTime).ToListAsync();
            await Clients.Caller.SendAsync("CurrentChatMessages", messages);
        }

        public async Task searchResult(string str, string id)
        {
            //int userid = Convert.ToInt32(id);
            //var currUser = await context.Users.Where(e => e.Id == Convert.ToInt32(userid)).FirstOrDefaultAsync();

            //var otherUserChat = context.Chats
            //.Where(x => x.Users.Contains(currUser))
            //.ToList();

            //await Clients.Caller.SendAsync("SearchResult", null);



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
        public async Task GetAccountImage(string id) 
        {
            User user = await context.Users.FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(id));

            await Clients.Caller.SendAsync("Image", user.Avatar);
        }


        public async Task CreateChat(int other, int id)
        {
            User user = await context.Users.FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(id));
            User otheruser = await context.Users.FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(other));

            Chat chat = new Chat();
            chat.Type = "Chat";

            AttachmentUserChat attachment1 = new AttachmentUserChat();
            attachment1.Chat = chat;
            attachment1.User = user;

            AttachmentUserChat attachment2 = new AttachmentUserChat();
            attachment2.Chat = chat;
            attachment2.User = otheruser;

            await context.AddAsync(chat);
            await context.AttachmentUserChats.AddRangeAsync(attachment1, attachment2);
            await context.SaveChangesAsync();


            var chatsToSend = await context.AttachmentUserChats
                .Include(a => a.Chat)
                .ThenInclude(e => e.Messages)
                .Include(a => a.User)
                .Where(e => e.ChatId == chat.Id)
                .GroupBy(a => a.Chat)
                .Select(g => new ChatToSend
                {
                    Id = g.Key.Id,
                    Type = g.Key.Type,
                    Users = g.Select(a => a.User).ToList(),
                    Messages = g.Select(a=>a.Chat.Messages).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            await Clients.Caller.SendAsync("NewChatData", chatsToSend);
        }

        public async Task DeleteChat(int id)
        {
            var chat = await context.Chats.FindAsync(id);
            var hh = await context.AttachmentUserChats.Where(e=>e.ChatId==id).ToListAsync();

            context.AttachmentUserChats.RemoveRange(hh);
            context.Chats.Remove(chat);
            await context.SaveChangesAsync();

            await Clients.Caller.SendAsync("Image", 1);
        }
    }
}
