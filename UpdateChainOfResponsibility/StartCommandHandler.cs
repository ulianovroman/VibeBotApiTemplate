using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class StartCommandHandler : IUpdateHandler
    {
        private readonly WordsToolContext _db;
        private readonly IBotMenuService _menuService;

        public StartCommandHandler(WordsToolContext db, IBotMenuService menuService)
        {
            _db = db;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var message = update.Message;
            if (!IsStartCommand(message?.Text))
            {
                await next();
                return;
            }

            var userId = context.User?.Id ?? message.From?.Id;
            if (userId is null)
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

            if (string.IsNullOrWhiteSpace(context.StudyLanguageCode))
            {
                await _menuService.ShowLanguageSelectionAsync(message.Chat.Id, messageId: null, ct);
                return;
            }

            await _menuService.RefreshMainMenuAsync(message.Chat.Id, context.StudyLanguageCode, ct);
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
