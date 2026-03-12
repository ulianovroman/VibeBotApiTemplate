using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class MyCardItemCallbackHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;

        public MyCardItemCallbackHandler(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            if (update.Type != UpdateType.CallbackQuery
                || update.CallbackQuery?.Data is not { } callbackData
                || !callbackData.StartsWith(BotCallbackData.MyCardsCardPrefix, StringComparison.Ordinal))
            {
                await next();
                return;
            }

            await _bot.TryAnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
        }
    }
}
