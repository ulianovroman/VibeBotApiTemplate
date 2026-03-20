using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeBotApi.Storage
{
    public class MessageLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int UpdateId { get; set; }

        public int? MessageId { get; set; }

        public long? ChatId { get; set; }

        public long SenderUserId { get; set; }

        [MaxLength(256)]
        public string Text { get; set; } = string.Empty;

        public DateTime Received { get; set; }

        public DateTime LoggedAt { get; set; }

        [MaxLength(64)]
        public string? SenderUsername { get; set; }
    }
}
