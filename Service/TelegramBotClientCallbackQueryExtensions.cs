using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace VibeBotApi.Service
{
    public static class TelegramBotClientCallbackQueryExtensions
    {
        public static async Task TryAnswerCallbackQuery(
            this ITelegramBotClient bot,
            string callbackQueryId,
            string? text = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await bot.AnswerCallbackQuery(
                    callbackQueryId: callbackQueryId,
                    text: text,
                    cancellationToken: cancellationToken);
            }
            catch (ApiRequestException ex) when (IsExpiredOrInvalidCallbackQuery(ex))
            {
                // Ignore stale callback query acknowledgements to avoid crashing the update pipeline.
            }
        }

        private static bool IsExpiredOrInvalidCallbackQuery(ApiRequestException ex)
        {
            return ex.ErrorCode == 400
                && (ex.Message.Contains("query is too old", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("response timeout expired", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("query ID is invalid", StringComparison.OrdinalIgnoreCase));
        }
    }
}
