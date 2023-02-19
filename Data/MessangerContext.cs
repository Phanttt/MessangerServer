using MessangerServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MessangerServer.Data
{
    public class MessangerContext : DbContext
    {
        public MessangerContext(DbContextOptions<MessangerContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AttachmentUserChat> AttachmentUserChats { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
