namespace MessangerServer.Models.ToClient
{
    public class ChatToSend
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IList<User> Users { get; set; } = new List<User>();
        public IList<Message>? Messages { get; set; } = new List<Message>();
    }
}
