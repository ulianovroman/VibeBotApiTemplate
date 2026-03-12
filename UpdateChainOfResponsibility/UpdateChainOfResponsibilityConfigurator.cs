namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public static class UpdateChainOfResponsibilityConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            // Order of handlers matters - they will be called in the same order as registered

            #region Context hydration
            services.AddScoped<IUpdateHandler, SetUserHandler>();
            services.AddScoped<IUpdateHandler, SetUserStudyLanguageHandler>();
            services.AddScoped<IUpdateHandler, LoggingHandler>();
            #endregion

            #region Media handlers
            services.AddScoped<IUpdateHandler, PhotoMessageHandler>();
            #endregion

            #region Callback handlers
            services.AddScoped<IUpdateHandler, ChangeStudyingLanguageCallbackHandler>();
            services.AddScoped<IUpdateHandler, MainMenuCallbackHandler>();
            services.AddScoped<IUpdateHandler, MyCardsCallbackHandler>();
            services.AddScoped<IUpdateHandler, StartMenuCallbackHandler>();
            services.AddScoped<IUpdateHandler, MyCardsAddCallbackHandler>();
            services.AddScoped<IUpdateHandler, MyCardItemCallbackHandler>();
            services.AddScoped<IUpdateHandler, StudyLanguageSelectionCallbackHandler>();
            #endregion

            #region Command and message handlers
            services.AddScoped<IUpdateHandler, StartCommandHandler>();
            services.AddScoped<IUpdateHandler, MenuCommandHandler>();
            services.AddScoped<IUpdateHandler, TextMessageHandler>();
            #endregion

            // Processor that will call handlers in order
            services.AddScoped<UpdateProcessor>();
        }
    }
}
