﻿namespace MessangerServer.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        public int User1Id { get; set; }
        public User User1 { get; set; }
        public int User2Id { get; set; }
        public User User2 { get; set; }
        public string LastMessage { get; set; }
        public int NotReaded { get; set; }
    }
}