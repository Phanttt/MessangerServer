using MessangerServer.Models;

namespace MessangerServer.Data
{
    public class ContextInitializer
    {
        public static void Init(MessangerContext context)
        {
            if (!context.Users.Any())
            {
                User u1 = new User()
                {
                    Name = "User1",
                    Login="userlogg",
                    Password="12345",
                    Status = true
                };
                User u2 = new User()
                {
                    Name = "Igor",
                    Login = "igoriok",
                    Password = "23325",
                    Status = false
                };
                User u3 = new User()
                {
                    Name = "Piperoni",
                    Login = "potaop",
                    Password = "97451",
                    Status = false
                };

                context.Users.AddRange(u1,u2,u3);

                Chat chat = new Chat()
                {
                    NotReaded = 1
                };
                Message message = new Message()
                {
                    ChatId = chat.Id,
                    Sender = u1,
                    Content = "Hello",
                    DispatchTime = DateTime.Now,
                    IsReaded = false
                };
                context.Messages.Add(message);
                
                chat.Users.Add(u1);
                chat.Users.Add(u2);
                chat.Messages.Add(message);
                context.Chats.Add(chat);
                

                AttachmentUserChat attachment1 = new AttachmentUserChat()
                {
                    User = u1,
                    Chat = chat
                };
                AttachmentUserChat attachment2 = new AttachmentUserChat()
                {
                    User = u2,
                    Chat = chat
                };
                context.AttachmentUserChats.AddRange(attachment1, attachment2);

                context.SaveChanges();
            }
        }
    }
}
