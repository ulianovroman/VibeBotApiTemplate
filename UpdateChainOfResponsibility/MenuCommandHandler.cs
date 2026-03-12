using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class MenuCommandHandler : IUpdateHandler
    {
        private readonly WordsToolContext _db;
        private readonly IBotMenuService _menuService;

        public MenuCommandHandler(WordsToolContext db, IBotMenuService menuService)
        {
            _db = db;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var message = update.Message;
            if (message?.Text != "/menu")
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

            await _menuService.RefreshMainMenuAsync(message.Chat.Id, context.StudyLanguageCode, ct);
        }
    }
}
