using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MessangerServer.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int ChatId { get; set; }

        public string Content { get; set; }
        public DateTime DispatchTime { get; set; }
        public bool IsReaded { get; set; }
    }

}
