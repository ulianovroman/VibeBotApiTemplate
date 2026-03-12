namespace BotApiTemplate.Service
{
    public static class BotPhrases
    {
        public const string SelectLanguageToStudy = "Выберите язык для изучения";
        public const string MainMenu = "Главное меню";
        public const string MyCards = "Мои карточки";

        public const string ButtonMyCards = "Мои карточки";
        public const string ButtonBack = "Назад";
        public const string ButtonStart = "Начать";

        public const string AddNewCards = "Добавить новые карточки";
        public const string PromptSendWordOrPhrase = "Отправьте слово или фразу текстом";
        public const string ButtonAlreadyHandled = "Кнопка уже обработана";
        public const string InvalidButton = "Некорректная кнопка";
        public const string SelectStudyLanguageFirst = "Сначала выберите язык изучения";
        public const string PhraseTooLongShort = "Фраза слишком длинная";
        public const string CardsLimitReachedShort = "Достигнут лимит карточек";
        public const string CardAdded = "Карточка добавлена";
        public const string AddErrorShort = "Ошибка при добавлении";
        public const string AddErrorRetry = "Не удалось добавить карточку. Попробуйте снова.";
        public const string AllCardsAlreadyAdded = "Все карточки из сообщения уже были добавлены ранее.";

        public static string PhraseTooLong(int maxLength) => $"Фраза слишком длинная. Максимум {maxLength} символов.";

        public static string CardsLimitReached(int maxCardsPerUser) =>
            $"Достигнут лимит карточек ({maxCardsPerUser}). Удали часть карточек, чтобы добавить новые.";

        public static string CardsLimitReachedShortWithCount(int maxCardsPerUser) =>
            $"Достигнут лимит карточек ({maxCardsPerUser}).";

        public static string BuildAddedCardsText(IEnumerable<string> addedItems) =>
            string.Join("\n", addedItems.Select(x => $"{x} ✅")) + "\n\n" + AddNewCards;
    }
}
