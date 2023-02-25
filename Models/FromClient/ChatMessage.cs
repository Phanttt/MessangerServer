namespace MessangerServer.Models.FromClient
{
    public class ChatMessage
    {
        public int ChatId { get; set; }
        public string Content { get; set; }
        public Sender Sender { get; set; }
        public DateTime? DispatchTime { get; set; }
        public bool IsReaded { get; set; }
    }
}
