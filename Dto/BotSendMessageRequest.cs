namespace VibeBotApi.Dto;

public sealed class BotSendMessageRequest : VibeBotApiSecretRequest
{
    public long? ChatId { get; set; }

    public string? Text { get; set; }
}
