using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class ChangeStudyingLanguageCallbackHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly IBotMenuService _menuService;

        public ChangeStudyingLanguageCallbackHandler(ITelegramBotClient bot, IBotMenuService menuService)
        {
            _bot = bot;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            if (update.Type != UpdateType.CallbackQuery
                || update.CallbackQuery?.Data != BotCallbackData.ChangeStudyingLanguage
                || update.CallbackQuery.Message?.Chat.Id is not { } chatId)
            {
                await next();
                return;
            }

            await _menuService.ShowLanguageSelectionAsync(
                chatId,
                update.CallbackQuery.Message.MessageId,
                ct);

            await _bot.TryAnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
        }
    }
}
