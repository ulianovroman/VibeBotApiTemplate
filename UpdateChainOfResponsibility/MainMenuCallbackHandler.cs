using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class MainMenuCallbackHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly IBotMenuService _menuService;

        public MainMenuCallbackHandler(ITelegramBotClient bot, IBotMenuService menuService)
        {
            _bot = bot;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var callbackQuery = update.CallbackQuery;
            var message = callbackQuery?.Message;

            if (update.Type != UpdateType.CallbackQuery
                || callbackQuery?.Data != BotCallbackData.MainMenu
                || message?.Chat.Id is not { } chatId)
            {
                await next();
                return;
            }

            await _menuService.ShowMainMenuAsync(chatId, context.StudyLanguageCode, message.MessageId, ct);
            await _bot.TryAnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }
    }
}
