using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class StartCommandHandler : IUpdateHandler
    {
        private readonly BotContext _db;
        private readonly ITelegramBotClient _bot;

        public StartCommandHandler(BotContext db, ITelegramBotClient bot)
        {
            _db = db;
            _bot = bot;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var message = update.Message;
            if (!IsStartCommand(message?.Text))
            {
                await next();
                return;
            }

            var userId = context.User?.Id ?? message?.From?.Id;
            if (userId is null || message is null)
            {
                await next();
                return;
            }

            var hasPermission = await _db.UserPermissions
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId.Value, ct);

            if (!hasPermission)
            {
                return;
            }

            await _bot.SendMessage(
                chatId: message.Chat.Id,
                text: "Бот запущен. Доступ подтверждён.",
                cancellationToken: ct);
        }

        private static bool IsStartCommand(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var command = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            if (!command.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return command.Length == 6 || (command.Length > 6 && command[6] == '@');
        }
    }
}
