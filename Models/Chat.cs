namespace MessangerServer.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string LastMessage { get; set; }
        public int NotReaded { get; set; }
        public DateTime LastTime { get; set; }
        public IList<User> Users { get; set; } = new List<User>();
        public IList<Message> Messages{ get; set; } = new List<Message>();

    }
}
