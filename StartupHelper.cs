using Microsoft.EntityFrameworkCore;
using Npgsql;
using Telegram.Bot;
using BotApiTemplate.Storage;
using BotApiTemplate.UpdateChainOfResponsibility;
using BotApiTemplate.Service;

namespace BotApiTemplate
{
    public static class StartupHelper
    {
        public static async Task RegisterDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            }

            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = uri.AbsolutePath.Trim('/'),
                SslMode = SslMode.Require
            };

            builder.Services.AddDbContext<WordsToolContext>(options =>
                options.UseNpgsql(connBuilder.ConnectionString));

            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (!string.IsNullOrWhiteSpace(telegramBotToken))
            {
                builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramBotToken));
            }

            builder.Services.AddOptions<GptOptions>()
                .Bind(builder.Configuration.GetSection(GptOptions.SectionName))
                .PostConfigure(options =>
                {
                    var apiKeyFromEnv = Environment.GetEnvironmentVariable("GPT_API_KEY");
                    var modelFromEnv = Environment.GetEnvironmentVariable("GPT_MODEL");

                    if (!string.IsNullOrWhiteSpace(apiKeyFromEnv))
                    {
                        options.ApiKey = apiKeyFromEnv;
                    }

                    if (!string.IsNullOrWhiteSpace(modelFromEnv))
                    {
                        options.Model = modelFromEnv;
                    }
                });
            builder.Services.AddSingleton<IGptService, GptService>();

            UpdateChainOfResponsibilityConfigurator.Configure(builder.Services);
        }

        public static async Task Init(WebApplication app)
        {
            var logger = app.Logger;

            logger.LogInformation("Own logs starts here");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WordsToolContext>();

                var migrations = db.Database.GetPendingMigrations();

                if (migrations.Any())
                {
                    logger.LogInformation("Migrating database");
                    logger.LogInformation("Pending migrations:");
                    foreach (var migration in migrations)
                    {
                        logger.LogInformation(migration);
                    }
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Migrated successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations");
                }

                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var domain = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN");
                var secret = Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET");

                await botClient.SetWebhook(
                    url: $"https://{domain}/api/telegram/webhook",
                    secretToken: secret,
                    cancellationToken: CancellationToken.None);

            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => "OK");
            app.Run();
        }
    }
}
