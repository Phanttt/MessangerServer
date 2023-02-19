namespace MessangerServer.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string LastMessage { get; set; }
        public int NotReaded { get; set; }
    }
}
