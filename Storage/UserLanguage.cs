namespace BotApiTemplate.Storage
{
    public class UserLanguage
    {
        public long UserId { get; set; }
        public string NativeLang { get; set; } = string.Empty;
        public string StudyLang { get; set; } = string.Empty;
    }
}
