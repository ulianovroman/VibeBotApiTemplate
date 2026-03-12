using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class StudyLanguageSelectionCallbackHandler : IUpdateHandler
    {
        private const string NativeLanguageCode = "RU";

        private readonly ITelegramBotClient _bot;
        private readonly WordsToolContext _db;
        private readonly IBotMenuService _menuService;

        public StudyLanguageSelectionCallbackHandler(ITelegramBotClient bot, WordsToolContext db, IBotMenuService menuService)
        {
            _bot = bot;
            _db = db;
            _menuService = menuService;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var callbackQuery = update.CallbackQuery;
            var message = callbackQuery?.Message;

            if (update.Type != UpdateType.CallbackQuery
                || callbackQuery?.Data is not { } callbackData
                || !callbackData.StartsWith(BotCallbackData.LanguageCallbackPrefix, StringComparison.Ordinal)
                || message?.Chat.Id is not { } chatId)
            {
                await next();
                return;
            }

            var userId = context.User?.Id ?? callbackQuery.From.Id;

            var studyLanguageCode = callbackData[BotCallbackData.LanguageCallbackPrefix.Length..];

            var existingLanguage = await _db.UserLanguages
                .SingleOrDefaultAsync(
                    x => x.UserId == userId
                         && x.NativeLang == NativeLanguageCode,
                    ct);

            if (existingLanguage is null)
            {
                _db.UserLanguages.Add(new UserLanguage
                {
                    UserId = userId,
                    NativeLang = NativeLanguageCode,
                    StudyLang = studyLanguageCode
                });
            }
            else if (!string.Equals(existingLanguage.StudyLang, studyLanguageCode, StringComparison.Ordinal))
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE ""UserLanguages"" 
                       SET ""StudyLang"" = {studyLanguageCode}
                       WHERE ""UserId"" = {userId} AND ""NativeLang"" = {NativeLanguageCode}",
                    ct);
            }

            await _db.SaveChangesAsync(ct);

            context.StudyLanguageCode = studyLanguageCode;

            await _menuService.ShowMainMenuAsync(chatId, studyLanguageCode, message.MessageId, ct);

            await _bot.TryAnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }
    }
}
