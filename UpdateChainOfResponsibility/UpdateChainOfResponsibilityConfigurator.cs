namespace VibeBotApi.UpdateChainOfResponsibility
{
    public static class UpdateChainOfResponsibilityConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddScoped<IUpdateHandler, SetUserHandler>();
            services.AddScoped<IUpdateHandler, LoggingHandler>();
            services.AddScoped<IUpdateHandler, StartCommandHandler>();

            services.AddScoped<UpdateProcessor>();
        }
    }
}
