using BotApiTemplate.Dto;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class UpdateContext
    {
        public UserDto? User { get; set; }
        public string? StudyLanguageCode { get; set; }
    }
}
