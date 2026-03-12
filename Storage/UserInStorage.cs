namespace BotApiTemplate.Storage
{
    public class UserInStorage
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public DateTime Created { get; set; }
    }
}
