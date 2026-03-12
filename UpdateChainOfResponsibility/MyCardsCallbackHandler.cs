using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class MyCardsCallbackHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly IBotMenuService _menuService;

        public MyCardsCallbackHandler(ITelegramBotClient bot, IBotMenuService menuService)
        {
            _bot = bot;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var callbackQuery = update.CallbackQuery;
            var message = callbackQuery?.Message;

            if (update.Type != UpdateType.CallbackQuery
                || callbackQuery?.Data is not { } callbackData
                || !IsMyCardsCallback(callbackData)
                || message?.Chat.Id is not { } chatId)
            {
                await next();
                return;
            }

            var userId = context.User?.Id ?? callbackQuery.From.Id;

            var page = ParsePage(callbackData);

            await _menuService.ShowMyCardsMenuAsync(
                chatId,
                userId,
                page,
                message.MessageId,
                ct);

            await _bot.TryAnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }

        private static bool IsMyCardsCallback(string callbackData)
        {
            return callbackData == BotCallbackData.MyCards
                   || callbackData.StartsWith(BotCallbackData.MyCardsPagePrefix, StringComparison.Ordinal);
        }

        private static int ParsePage(string callbackData)
        {
            if (!callbackData.StartsWith(BotCallbackData.MyCardsPagePrefix, StringComparison.Ordinal))
            {
                return 0;
            }

            var rawPage = callbackData[BotCallbackData.MyCardsPagePrefix.Length..];
            return int.TryParse(rawPage, out var page) && page >= 0 ? page : 0;
        }
    }
}
