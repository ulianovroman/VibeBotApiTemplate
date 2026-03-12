namespace BotApiTemplate.Service
{
    public interface IBotMenuService
    {
        Task ShowLanguageSelectionAsync(long chatId, int? messageId, CancellationToken ct);
        Task ShowMainMenuAsync(long chatId, string? studyLanguageCode, int? messageId, CancellationToken ct);
        Task ShowMyCardsMenuAsync(long chatId, long userId, int page, int? messageId, CancellationToken ct);
        Task RefreshMainMenuAsync(long chatId, string? studyLanguageCode, CancellationToken ct);
    }
}
