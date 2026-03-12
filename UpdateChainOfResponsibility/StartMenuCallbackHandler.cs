using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class StartMenuCallbackHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;

        public StartMenuCallbackHandler(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            if (update.Type != UpdateType.CallbackQuery
                || update.CallbackQuery?.Data != BotCallbackData.Start)
            {
                await next();
                return;
            }

            await _bot.TryAnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
        }
    }
}
