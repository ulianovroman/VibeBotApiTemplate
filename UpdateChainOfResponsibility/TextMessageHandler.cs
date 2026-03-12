using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class TextMessageHandler : IUpdateHandler
    {
        private const int MaxPhraseLength = 100;

        private readonly ITelegramBotClient _bot;
        private readonly WordsToolContext _db;

        public TextMessageHandler(
            ITelegramBotClient bot,
            WordsToolContext db)
        {
            _bot = bot;
            _db = db;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            var message = update.Message;

            if (message?.Text is null)
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
                .AnyAsync(p => p.UserId == userId.Value, ct);

            if (!hasPermission)
            {
                await next();
                return;
            }

            if (message.Text.StartsWith("/", StringComparison.Ordinal))
            {
                await next();
                return;
            }

            if (string.IsNullOrWhiteSpace(context.StudyLanguageCode))
            {
                await next();
                return;
            }

            var inputText = message.Text.Trim();

            if (inputText.Length > MaxPhraseLength)
            {
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: BotPhrases.PhraseTooLong(MaxPhraseLength),
                    cancellationToken: ct);

                return;
            }

            var maxCardsPerUser = ReadMaxCardsPerUser();
            var currentCardsCount = await _db.Cards
                .AsNoTracking()
                .CountAsync(x => x.CreatedUserId == userId.Value, ct);

            if (currentCardsCount >= maxCardsPerUser)
            {
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: BotPhrases.CardsLimitReached(maxCardsPerUser),
                    cancellationToken: ct);

                return;
            }

            var pendingItems = await BuildPendingItemsAsync(userId.Value, inputText, ct);
            if (pendingItems.Count == 0)
            {
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: BotPhrases.AllCardsAlreadyAdded,
                    cancellationToken: ct);

                return;
            }

            await _bot.SendMessage(
                chatId: message.Chat.Id,
                text: BotPhrases.AddNewCards,
                replyMarkup: BuildKeyboard(pendingItems),
                cancellationToken: ct);
        }

        private async Task<Dictionary<string, string>> BuildPendingItemsAsync(long userId, string phrase, CancellationToken ct)
        {
            var words = Regex.Split(phrase, @"[\s\p{P}\p{S}]+")
                .Select(x => x.Trim())
                .Where(x => x.Length > 1)
                .Where(x => !x.All(char.IsDigit))
                .Where(x => x.Any(char.IsLetter))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Dictionary<string, string> candidateItems;

            if (words.Count == 1)
            {
                candidateItems = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["w0"] = words[0]
                };
            }
            else
            {
                candidateItems = new[] { KeyValuePair.Create("p", phrase) }
                    .Concat(words.Select((word, index) => KeyValuePair.Create($"w{index}", word)))
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            }

            var normalizedCandidates = candidateItems
                .Select(x => NormalizeText(x.Value))
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var existingForUser = await _db.UserCards
                .AsNoTracking()
                .Where(uc => uc.UserId == userId)
                .Join(
                    _db.Cards.AsNoTracking(),
                    uc => uc.CardId,
                    c => c.Id,
                    (_, c) => new { c.OriginalVersion, c.EnglishVersion })
                .Where(c => normalizedCandidates.Contains(c.OriginalVersion.ToLower()) || normalizedCandidates.Contains(c.EnglishVersion.ToLower()))
                .ToListAsync(ct);

            var existingSet = existingForUser
                .SelectMany(x => new[] { x.OriginalVersion, x.EnglishVersion })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeText)
                .ToHashSet(StringComparer.Ordinal);

            return candidateItems
                .Where(x => !existingSet.Contains(NormalizeText(x.Value)))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        }

        private static InlineKeyboardMarkup BuildKeyboard(Dictionary<string, string> pendingItems)
        {
            var rows = new List<List<InlineKeyboardButton>>();

            if (pendingItems.TryGetValue("p", out var phrase))
            {
                rows.Add([
                    InlineKeyboardButton.WithCallbackData($"➕ {phrase}", BotCallbackData.AddNewCardPrefix + "p")
                ]);
            }

            var wordButtons = pendingItems
                .Where(x => x.Key.StartsWith("w", StringComparison.Ordinal))
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => InlineKeyboardButton.WithCallbackData($"➕ {x.Value}", BotCallbackData.AddNewCardPrefix + x.Key))
                .ToList();

            if (wordButtons.Count > 0)
            {
                rows.Add(wordButtons);
            }

            return new InlineKeyboardMarkup(rows);
        }

        private static string NormalizeText(string text)
        {
            return text.Trim().ToLowerInvariant();
        }

        private static int ReadMaxCardsPerUser()
        {
            var value = Environment.GetEnvironmentVariable("MAX_CARDS_PER_USER");
            return int.TryParse(value, out var parsed) && parsed > 0
                ? parsed
                : 100;
        }
    }
}
