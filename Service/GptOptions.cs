namespace BotApiTemplate.Service
{
    public sealed class GptOptions
    {
        public const string SectionName = "Gpt";

        public string? ApiKey { get; set; }
        public string? Model { get; set; }
    }
}
