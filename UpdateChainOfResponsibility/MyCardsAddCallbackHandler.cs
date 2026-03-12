using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class MyCardsAddCallbackHandler : IUpdateHandler
    {
        private const int MaxPhraseLength = 100;
        private const string NativeLanguageCode = "RU";

        private readonly ITelegramBotClient _bot;
        private readonly WordsToolContext _db;
        private readonly IGptService _gptService;
        private readonly ILogger<MyCardsAddCallbackHandler> _logger;

        public MyCardsAddCallbackHandler(
            ITelegramBotClient bot,
            WordsToolContext db,
            IGptService gptService,
            ILogger<MyCardsAddCallbackHandler> logger)
        {
            _bot = bot;
            _db = db;
            _gptService = gptService;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            if (update.Type != UpdateType.CallbackQuery || update.CallbackQuery is not { } callback)
            {
                await next();
                return;
            }

            if (callback.Data == BotCallbackData.MyCardsAdd)
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.PromptSendWordOrPhrase, cancellationToken: ct);
                return;
            }

            if (callback.Data is not { } callbackData
                || !callbackData.StartsWith(BotCallbackData.AddNewCardPrefix, StringComparison.Ordinal)
                || callback.Message is not { } callbackMessage)
            {
                await next();
                return;
            }

            var selectedButton = FindButtonByCallbackData(callbackMessage.ReplyMarkup, callbackData);
            if (selectedButton is null)
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.ButtonAlreadyHandled, cancellationToken: ct);
                return;
            }

            var selectedText = ExtractTextFromButton(selectedButton.Text);
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.InvalidButton, cancellationToken: ct);
                return;
            }

            var userId = context.User?.Id ?? callback.From.Id;

            var studyLanguageCode = context.StudyLanguageCode;

            if (string.IsNullOrWhiteSpace(studyLanguageCode))
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.SelectStudyLanguageFirst, cancellationToken: ct);
                return;
            }

            if (selectedText.Length > MaxPhraseLength)
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.PhraseTooLongShort, cancellationToken: ct);
                await _bot.SendMessage(callbackMessage.Chat.Id, BotPhrases.PhraseTooLong(MaxPhraseLength), cancellationToken: ct);
                return;
            }

            var maxCardsPerUser = ReadMaxCardsPerUser();
            var currentCardsCount = await _db.Cards
                .AsNoTracking()
                .CountAsync(x => x.CreatedUserId == userId, ct);

            if (currentCardsCount >= maxCardsPerUser)
            {
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.CardsLimitReachedShort, cancellationToken: ct);
                await _bot.SendMessage(callbackMessage.Chat.Id, BotPhrases.CardsLimitReachedShortWithCount(maxCardsPerUser), cancellationToken: ct);
                return;
            }

            try
            {
                var normalizedSelectedText = NormalizeText(selectedText);

                var existingCardId = await _db.Cards
                    .AsNoTracking()
                    .Where(c => c.OriginalVersion.ToLower() == normalizedSelectedText || c.EnglishVersion.ToLower() == normalizedSelectedText)
                    .OrderBy(c => c.Id)
                    .Select(c => (long?)c.Id)
                    .FirstOrDefaultAsync(ct);

                long cardId;
                if (existingCardId.HasValue)
                {
                    cardId = existingCardId.Value;
                }
                else
                {
                    var cardData = await _gptService.GetCardData(
                        nativeLang: ToLanguageName(NativeLanguageCode),
                        studyLang: ToLanguageName(studyLanguageCode),
                        word: selectedText);

                    var newCard = new Card
                    {
                        SourceLang = NativeLanguageCode,
                        TargetLang = studyLanguageCode,
                        OriginalVersion = selectedText,
                        EnglishVersion = studyLanguageCode.Equals("EN", StringComparison.OrdinalIgnoreCase)
                            ? selectedText
                            : string.Empty,
                        Translation = cardData.t ?? string.Empty,
                        AddInfo = BuildAddInfo(cardData),
                        PartOfSpeech = cardData.pos ?? string.Empty,
                        Level = 0,
                        CreatedUserId = userId,
                        Created = DateTime.UtcNow
                    };

                    _db.Cards.Add(newCard);
                    await _db.SaveChangesAsync(ct);
                    cardId = newCard.Id;
                }

                var hasUserCardLink = await _db.UserCards
                    .AsNoTracking()
                    .AnyAsync(x => x.UserId == userId && x.CardId == cardId, ct);

                if (!hasUserCardLink)
                {
                    var nextStackId = await _db.UserCards
                        .AsNoTracking()
                        .Where(x => x.UserId == userId)
                        .Select(x => (long?)x.StackId)
                        .MaxAsync(ct) ?? 0;

                    _db.UserCards.Add(new UserCard
                    {
                        UserId = userId,
                        CardId = cardId,
                        StackId = nextStackId + 1,
                        Status = "New"
                    });

                    await _db.SaveChangesAsync(ct);
                }

                var updatedKeyboard = RemoveButton(callbackMessage.ReplyMarkup, callbackData);
                var updatedText = BuildUpdatedMessageText(callbackMessage.Text ?? BotPhrases.AddNewCards, selectedText);

                try
                {
                    await _bot.EditMessageText(
                        chatId: callbackMessage.Chat.Id,
                        messageId: callbackMessage.MessageId,
                        text: updatedText,
                        replyMarkup: updatedKeyboard,
                        cancellationToken: ct);
                }
                catch (ApiRequestException ex) when (IsMessageNotModified(ex))
                {
                    _logger.LogDebug(
                        ex,
                        "Skipped message edit because Telegram reported no changes for chat {ChatId}, message {MessageId}",
                        callbackMessage.Chat.Id,
                        callbackMessage.MessageId);
                }

                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.CardAdded, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create card for user {UserId}", userId);
                await _bot.TryAnswerCallbackQuery(callback.Id, BotPhrases.AddErrorShort, cancellationToken: ct);
                await _bot.SendMessage(callbackMessage.Chat.Id, BotPhrases.AddErrorRetry, cancellationToken: ct);
            }
        }

        private static InlineKeyboardButton? FindButtonByCallbackData(InlineKeyboardMarkup? markup, string callbackData)
        {
            return markup?.InlineKeyboard
                .SelectMany(row => row)
                .FirstOrDefault(button => string.Equals(button.CallbackData, callbackData, StringComparison.Ordinal));
        }

        private static InlineKeyboardMarkup? RemoveButton(InlineKeyboardMarkup? markup, string callbackData)
        {
            if (markup is null)
            {
                return null;
            }

            var rows = markup.InlineKeyboard
                .Select(row => row.Where(button => !string.Equals(button.CallbackData, callbackData, StringComparison.Ordinal)).ToList())
                .Where(row => row.Count > 0)
                .Select(row => row.ToArray())
                .ToArray();

            return rows.Length == 0 ? null : new InlineKeyboardMarkup(rows);
        }

        private static string ExtractTextFromButton(string buttonText)
        {
            const string prefix = "➕ ";
            return buttonText.StartsWith(prefix, StringComparison.Ordinal)
                ? buttonText[prefix.Length..].Trim()
                : buttonText.Trim();
        }

        private static string BuildUpdatedMessageText(string currentText, string selectedText)
        {
            var addedItems = currentText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.EndsWith("✅", StringComparison.Ordinal))
                .Select(x => x[..^1].Trim())
                .ToList();

            if (!addedItems.Contains(selectedText, StringComparer.Ordinal))
            {
                addedItems.Add(selectedText);
            }

            return BotPhrases.BuildAddedCardsText(addedItems);
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

        private static string ToLanguageName(string languageCode)
        {
            return languageCode.ToUpperInvariant() switch
            {
                "RU" => "Russian",
                "EN" => "English",
                "EL" => "Greek",
                _ => languageCode
            };
        }

        private static string BuildAddInfo(Dto.CardDto cardData)
        {
            var forms = cardData.forms is { Count: > 0 }
                ? string.Join(", ", cardData.forms)
                : string.Empty;
            var examples = cardData.ex is { Count: > 0 }
                ? string.Join(" | ", cardData.ex)
                : string.Empty;

            return $"m: {cardData.m ?? string.Empty}; forms: {forms}; ex: {examples}";
        }

        private static bool IsMessageNotModified(ApiRequestException ex)
        {
            return ex.ErrorCode == 400
                && ex.Message.Contains("message is not modified", StringComparison.OrdinalIgnoreCase);
        }
    }
}
