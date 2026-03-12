namespace BotApiTemplate.Service
{
    public static class BotCallbackData
    {
        public const string LanguageCallbackPrefix = "study_lang:";
        public const string EnglishLanguage = LanguageCallbackPrefix + "EN";
        public const string GreekLanguage = LanguageCallbackPrefix + "EL";
        public const string ChangeStudyingLanguage = "menu:change_lang";
        public const string MyCards = "menu:my_cards";
        public const string MainMenu = "menu:main";
        public const string Start = "menu:start";
        public const string MyCardsAdd = "menu:my_cards:add";
        public const string AddNewCardPrefix = "cards:add_new:";
        public const string MyCardsPagePrefix = "menu:my_cards:page:";
        public const string MyCardsCardPrefix = "menu:my_cards:card:";
    }
}
