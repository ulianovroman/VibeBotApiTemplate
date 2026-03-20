using Telegram.Bot.Types;
using VibeBotApi.Dto;
using VibeBotApi.Storage;

namespace VibeBotApi.UpdateChainOfResponsibility
{
    public sealed class SetUserHandler : IUpdateHandler
    {
        private readonly BotContext _db;
        private readonly ILogger<SetUserHandler> _logger;

        public SetUserHandler(BotContext db, ILogger<SetUserHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var telegramUser = update.Message?.From
                               ?? update.CallbackQuery?.From
                               ?? update.EditedMessage?.From;

            if (telegramUser is null)
            {
                _logger.LogWarning("Skip update {UpdateId}: Telegram user not found in update", update.Id);
                return;
            }

            try
            {
                var existingUser = await _db.Users.FindAsync(new object[] { telegramUser.Id }, ct);

                if (existingUser is null)
                {
                    existingUser = new UserInStorage
                    {
                        Id = telegramUser.Id,
                        FirstName = telegramUser.FirstName,
                        LastName = telegramUser.LastName,
                        Username = telegramUser.Username,
                        Created = DateTime.UtcNow
                    };

                    _db.Users.Add(existingUser);
                }
                else
                {
                    existingUser.FirstName = telegramUser.FirstName;
                    existingUser.LastName = telegramUser.LastName;
                    existingUser.Username = telegramUser.Username;
                }

                await _db.SaveChangesAsync(ct);

                context.User = new UserDto
                {
                    Id = existingUser.Id,
                    FirstName = existingUser.FirstName,
                    LastName = existingUser.LastName,
                    Username = existingUser.Username,
                    Created = existingUser.Created
                };

                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist user {UserId} for update {UpdateId}", telegramUser.Id, update.Id);
                throw;
            }
        }
    }
}
