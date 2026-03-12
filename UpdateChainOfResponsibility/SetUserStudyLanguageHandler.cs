using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class SetUserStudyLanguageHandler : IUpdateHandler
    {
        private const string NativeLanguageCode = "RU";

        private readonly WordsToolContext _db;

        public SetUserStudyLanguageHandler(WordsToolContext db)
        {
            _db = db;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var userId = context.User?.Id
                         ?? update.Message?.From?.Id
                         ?? update.CallbackQuery?.From.Id
                         ?? update.EditedMessage?.From?.Id;

            if (userId is not null)
            {
                context.StudyLanguageCode = await _db.UserLanguages
                    .AsNoTracking()
                    .Where(x => x.UserId == userId.Value && x.NativeLang == NativeLanguageCode)
                    .Select(x => x.StudyLang)
                    .SingleOrDefaultAsync(ct);
            }

            await next();
        }
    }
}
