using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using BotApiTemplate.Storage;

namespace BotApiTemplate.Service
{
    public sealed class BotMenuService : IBotMenuService
    {
        private const string MenuMarkerText = "__bot_menu__";

        private readonly ITelegramBotClient _bot;
        private readonly WordsToolContext _db;

        public BotMenuService(ITelegramBotClient bot, WordsToolContext db)
        {
            _bot = bot;
            _db = db;
        }

        public async Task ShowLanguageSelectionAsync(long chatId, int? messageId, CancellationToken ct)
        {
            var keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData("🇬🇧", BotCallbackData.EnglishLanguage),
                    InlineKeyboardButton.WithCallbackData("🇬🇷", BotCallbackData.GreekLanguage)
                ]
            ]);

            await SendOrEditMenuAsync(chatId, messageId, BotPhrases.SelectLanguageToStudy, keyboard, ct);
        }

        public async Task ShowMainMenuAsync(long chatId, string? studyLanguageCode, int? messageId, CancellationToken ct)
        {
            var languageFlag = GetLanguageFlag(studyLanguageCode);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData(languageFlag, BotCallbackData.ChangeStudyingLanguage)],
                [InlineKeyboardButton.WithCallbackData(BotPhrases.ButtonMyCards, BotCallbackData.MyCards)],
                [InlineKeyboardButton.WithCallbackData(BotPhrases.ButtonStart, BotCallbackData.Start)]
            ]);

            await SendOrEditMenuAsync(chatId, messageId, BotPhrases.MainMenu, keyboard, ct);
        }

        public async Task RefreshMainMenuAsync(long chatId, string? studyLanguageCode, CancellationToken ct)
        {
            await DeleteTrackedMenuMessagesAsync(chatId, ct);
            await ShowMainMenuAsync(chatId, studyLanguageCode, null, ct);
        }

        public async Task ShowMyCardsMenuAsync(long chatId, long userId, int page, int? messageId, CancellationToken ct)
        {
            var safePage = Math.Max(0, page);
            const int pageSize = 9;

            var totalCount = await _db.UserCards.AsNoTracking().CountAsync(x => x.UserId == userId, ct);
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (safePage >= totalPages) safePage = totalPages - 1;

            var items = await _db.UserCards
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.StackId)
                .ThenBy(x => x.CardId)
                .Skip(safePage * pageSize)
                .Take(pageSize)
                .Join(_db.Cards.AsNoTracking(), uc => uc.CardId, c => c.Id, (uc, c) => new
                {
                    uc.CardId,
                    Text = string.IsNullOrWhiteSpace(c.OriginalVersion) ? c.EnglishVersion : c.OriginalVersion
                })
                .ToListAsync(ct);

            var rows = new List<List<InlineKeyboardButton>>();
            for (var i = 0; i < items.Count; i += 3)
            {
                rows.Add(items.Skip(i).Take(3)
                    .Select(x => InlineKeyboardButton.WithCallbackData(x.Text, BotCallbackData.MyCardsCardPrefix + x.CardId))
                    .ToList());
            }

            var paging = new List<InlineKeyboardButton>();
            if (safePage > 0) paging.Add(InlineKeyboardButton.WithCallbackData("⬅️", BotCallbackData.MyCardsPagePrefix + (safePage - 1)));
            if (safePage < totalPages - 1) paging.Add(InlineKeyboardButton.WithCallbackData("➡️", BotCallbackData.MyCardsPagePrefix + (safePage + 1)));
            if (paging.Count > 0) rows.Add(paging);

            rows.Add([InlineKeyboardButton.WithCallbackData(BotPhrases.ButtonBack, BotCallbackData.MainMenu)]);

            await SendOrEditMenuAsync(chatId, messageId, BotPhrases.MyCards, new InlineKeyboardMarkup(rows), ct);
        }

        private async Task SendOrEditMenuAsync(long chatId, int? messageId, string text, InlineKeyboardMarkup keyboard, CancellationToken ct)
        {
            if (messageId.HasValue)
            {
                await _bot.EditMessageText(chatId, messageId.Value, text, replyMarkup: keyboard, cancellationToken: ct);
                await TrackMenuMessageAsync(chatId, messageId.Value, ct);
                return;
            }

            await DeleteTrackedMenuMessagesAsync(chatId, ct);

            var sent = await _bot.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
            await TrackMenuMessageAsync(chatId, sent.MessageId, ct);
        }

        private async Task TrackMenuMessageAsync(long chatId, int messageId, CancellationToken ct)
        {
            var exists = await _db.MessageLogs.AsNoTracking().AnyAsync(x => x.ChatId == chatId && x.MessageId == messageId && x.SenderUserId == 0 && x.Text == MenuMarkerText, ct);
            if (exists) return;

            _db.MessageLogs.Add(new MessageLog
            {
                UpdateId = 0,
                MessageId = messageId,
                ChatId = chatId,
                SenderUserId = 0,
                SenderUsername = "bot",
                Text = MenuMarkerText,
                Received = DateTime.UtcNow,
                LoggedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        private async Task DeleteTrackedMenuMessagesAsync(long chatId, CancellationToken ct)
        {
            var tracked = await _db.MessageLogs
                .Where(x => x.ChatId == chatId && x.SenderUserId == 0 && x.Text == MenuMarkerText && x.MessageId != null)
                .ToListAsync(ct);

            foreach (var msg in tracked)
            {
                try
                {
                    await _bot.DeleteMessage(chatId, msg.MessageId!.Value, ct);
                }
                catch (ApiRequestException)
                {
                }
            }

            if (tracked.Count > 0)
            {
                _db.MessageLogs.RemoveRange(tracked);
                await _db.SaveChangesAsync(ct);
            }
        }

        private static string GetLanguageFlag(string? studyLanguageCode)
        {
            return studyLanguageCode?.ToUpperInvariant() switch
            {
                "EL" => "🇬🇷",
                _ => "🇬🇧"
            };
        }
    }
}
