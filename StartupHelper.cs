using Microsoft.EntityFrameworkCore;
using Npgsql;
using Telegram.Bot;
using VibeBotApi.Storage;
using VibeBotApi.UpdateChainOfResponsibility;
using VibeBotApi.Service;
using VibeBotApi.Jobs;
using System.Text;

namespace VibeBotApi
{
    public static class StartupHelper
    {
        public static async Task RegisterDependencies(WebApplicationBuilder builder)
        {
            ValidateStartupEnvironment();

            var telegramBotToken = Environment.GetEnvironmentVariable(EnvironmentVariables.TelegramBotToken)!;

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var databaseUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.DatabaseUrl)!;

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

            builder.Services.AddDbContext<BotContext>(options =>
                options.UseNpgsql(connBuilder.ConnectionString));

            builder.Services.AddAttributedQuartzJobs(typeof(StartupHelper).Assembly);
            builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramBotToken));

            builder.Services.AddOptions<GptOptions>()
                .Bind(builder.Configuration.GetSection(GptOptions.SectionName))
                .PostConfigure(options =>
                {
                    var apiKeyFromEnv = Environment.GetEnvironmentVariable(EnvironmentVariables.GptApiKey);
                    var modelFromEnv = Environment.GetEnvironmentVariable(EnvironmentVariables.GptModel);

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

        private static void ValidateStartupEnvironment()
        {
            var errors = new List<string>();

            ValidateRequiredEnvironmentVariable(EnvironmentVariables.TelegramBotToken, errors);
            ValidateRequiredEnvironmentVariable(EnvironmentVariables.DatabaseUrl, errors);
            ValidateRequiredEnvironmentVariable(EnvironmentVariables.RailwayPublicDomain, errors);
            ValidateRequiredEnvironmentVariable(EnvironmentVariables.TelegramWebhookSecret, errors);

            var databaseUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.DatabaseUrl);
            if (!string.IsNullOrWhiteSpace(databaseUrl) && !TryValidateDatabaseUrl(databaseUrl, out var databaseUrlError))
            {
                errors.Add(databaseUrlError);
            }

            if (errors.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Startup configuration validation failed.");
                builder.AppendLine("Please check required environment variables:");

                for (var i = 0; i < errors.Count; i++)
                {
                    builder.AppendLine($"{i + 1}. {errors[i]}");
                }

                throw new InvalidOperationException(builder.ToString());
            }
        }

        private static void ValidateRequiredEnvironmentVariable(string variableName, List<string> errors)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{variableName} is missing or empty.");
            }
        }

        private static bool TryValidateDatabaseUrl(string databaseUrl, out string error)
        {
            error = string.Empty;

            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} has invalid URI format. Expected: postgres://<user>:<password>@<host>:<port>/<db_name>.";
                return false;
            }

            var supportedSchemes = new[] { "postgres", "postgresql" };
            if (!supportedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must use postgres or postgresql scheme.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.UserInfo) || !uri.UserInfo.Contains(':', StringComparison.Ordinal))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must include user and password in user info section.";
                return false;
            }

            var userInfoParts = uri.UserInfo.Split(':', 2);
            if (userInfoParts.Length != 2 || string.IsNullOrWhiteSpace(userInfoParts[0]) || string.IsNullOrWhiteSpace(userInfoParts[1]))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must contain non-empty user and password.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must include host.";
                return false;
            }

            if (uri.Port <= 0)
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must include a valid port.";
                return false;
            }

            var dbName = uri.AbsolutePath.Trim('/');
            if (string.IsNullOrWhiteSpace(dbName))
            {
                error = $"{EnvironmentVariables.DatabaseUrl} must include database name in path.";
                return false;
            }

            return true;
        }

        public static async Task Init(WebApplication app)
        {
            var logger = app.Logger;

            logger.LogInformation("Own logs starts here");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BotContext>();

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
                var domain = Environment.GetEnvironmentVariable(EnvironmentVariables.RailwayPublicDomain);
                var secret = Environment.GetEnvironmentVariable(EnvironmentVariables.TelegramWebhookSecret);

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
