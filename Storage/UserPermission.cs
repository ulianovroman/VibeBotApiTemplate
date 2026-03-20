namespace VibeBotApi.Storage
{
    using System.ComponentModel.DataAnnotations;

    public class UserPermission
    {
        [Key]
        public long UserId { get; set; }
    }
}
