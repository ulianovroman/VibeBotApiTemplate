namespace BotApiTemplate.Storage
{
    public class UserCard
    {
        public long UserId { get; set; }
        public long StackId { get; set; }
        public long CardId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
