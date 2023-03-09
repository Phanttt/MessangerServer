using Microsoft.AspNetCore.SignalR;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;
using MessangerServer.Models.FromClient;
using MessangerServer.Models;
using MessangerServer.Models.ToClient;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MessangerServer.Controllers
{
    public class DataHub : Hub
    {
        private MessangerContext context;
        private static ConcurrentDictionary<int, string> _dictionary = new ConcurrentDictionary<int, string>();

        public DataHub(MessangerContext context)
        {
            this.context = context;
            //ContextInitializer.Init(this.context);
        }
        public override async Task OnConnectedAsync()
        {
            var userid = Convert.ToInt32(Context.GetHttpContext().Request.Query["userid"]);
            var chats = await context.AttachmentUserChats.Where(x => x.UserId == userid).ToListAsync();

            _dictionary.AddOrUpdate(userid, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
            

            foreach (var item in chats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Group" + item.ChatId);
                await Clients.Group("Group" + item.ChatId).SendAsync("UserConnect", userid);
            }

            var user = await context.Users.FirstOrDefaultAsync(e => e.Id == userid);
            if (user!=null)
            {
                user.Status = true;
            }

            await context.SaveChangesAsync();
            await base.OnConnectedAsync();
        }

        public async Task Disconnect(int id)
        {
            var chats = await context.AttachmentUserChats.Where(x => x.UserId == id).ToListAsync();

            _dictionary.TryRemove(id, out var removedValue);

            foreach (var item in chats)
            {
                await Clients.Group("Group" + item.ChatId).SendAsync("UserDisconnect", id);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Group" + item.ChatId);
            }

            var user = await context.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user != null)
            {
                user.Status = false;
            }
            await context.SaveChangesAsync();
        }

        public async Task SendMessage(ChatMessage message)
        {
            if (message.Id!=null)
            {
                Message mes = await context.Messages.FirstOrDefaultAsync(e => e.Id == message.Id);
                mes.Content = message.Content;
                context.Update(mes);
                await context.SaveChangesAsync();

                await Clients.Group("Group" + message.ChatId).SendAsync("OnEditMessage", mes);
            }
            else
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
            
        }
        public async Task GetChats(int currUserId)
        {
            var chats = await context.AttachmentUserChats
                .Where(e => e.UserId == currUserId)
                .Select(e=>e.ChatId)
                .ToListAsync();


            var chatsToSend = await context.AttachmentUserChats
                .Where(e => chats.Contains(e.ChatId))
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

        public async Task searchResult(string str, int userId)
        {
            if (!string.IsNullOrEmpty(str))
            {
                var chatUserIds = context.AttachmentUserChats
                .Where(auc => auc.UserId == userId)
                .Select(auc => auc.ChatId)
                .Distinct()
                .ToList();

                var usersWithoutChat = context.Users
                    .Where(u => u.Login.StartsWith(str) && u.Id != userId && !context.AttachmentUserChats
                        .Any(auc => chatUserIds.Contains(auc.ChatId) && auc.UserId == u.Id))
                    .ToList();

                await Clients.Caller.SendAsync("SearchResult", usersWithoutChat);
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


            string otherConnId;
            bool isExistOther = _dictionary.TryGetValue(otheruser.Id, out otherConnId);

            string currConnId;
            bool isExistCurr = _dictionary.TryGetValue(user.Id, out currConnId);

            if (isExistOther)
            {
                await Groups.AddToGroupAsync(otherConnId, "Group" + chat.Id);
            }

            if (isExistCurr)
            {
                await Groups.AddToGroupAsync(currConnId, "Group" + chat.Id);
            }
           
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

            await Clients.Group("Group" + chat.Id).SendAsync("NewChatData", chatsToSend);
        }

        public async Task DeleteChat(int id)
        {
            var chat = await context.Chats.FindAsync(id);
            var hh = await context.AttachmentUserChats.Where(e=>e.ChatId==id).ToListAsync();

            context.AttachmentUserChats.RemoveRange(hh);
            context.Chats.Remove(chat);
            await context.SaveChangesAsync();

            await Clients.Group("Group" + id).SendAsync("ChatDeleted", id); 
        }

        public async Task DeleteMessage(int id)
        {
            var messsage = await context.Messages.FindAsync(id);         
            context.Messages.Remove(messsage);

            await context.SaveChangesAsync();
            await Clients.Group("Group" + messsage.ChatId).SendAsync("OnDeleteMessage", id);
        }

        public async Task EditMassage(int id, string str)
        {
            var messsage = await context.Messages.FindAsync(id);
            messsage.Content = str;
            context.Messages.Remove(messsage);

            await context.SaveChangesAsync();
            await Clients.Group("Group" + messsage.ChatId).SendAsync("OnEditMessage", id);
        }
    }
}
