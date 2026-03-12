using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenAI;
using Telegram.Bot;
using BotApiTemplate.Service;
using BotApiTemplate.Storage;
using BotApiTemplate.UpdateChainOfResponsibility;

namespace BotApiTemplate
{
    public static class StartupHelper
    {
        public static async Task RegisterDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

            var s3Endpoint = Environment.GetEnvironmentVariable("S3_ENDPOINT");
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            builder.Services.AddSingleton<IAmazonS3>(_ =>
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                var config = new AmazonS3Config
                {
                    ServiceURL = s3Endpoint,
                    ForcePathStyle = true,
                    SignatureVersion = "4"
                };

                DisableChunkEncodingIfSupported(config);
                return new AmazonS3Client(credentials, config);
            });

            builder.Services.AddSingleton(_ =>
            {
                var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                return new OpenAIClient(apiKey);
            });

            UpdateChainOfResponsibilityConfigurator.Configure(builder.Services);

            builder.Services.AddSingleton<IS3BucketService, S3BucketService>();
            builder.Services.AddSingleton<IGptService, GptService>();
            builder.Services.AddScoped<IBotMenuService, BotMenuService>();
        }


        private static void DisableChunkEncodingIfSupported(AmazonS3Config config)
        {
            var property = config.GetType().GetProperty("UseChunkEncoding");
            if (property?.CanWrite == true && property.PropertyType == typeof(bool))
            {
                property.SetValue(config, false);
            }
        }

        public static async Task Init(WebApplication app)
        {
            var logger = app.Logger;

            logger.LogInformation($"Own logs starts here");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WordsToolContext>();

                var migrations = db.Database.GetPendingMigrations();

                if (migrations.Any())
                {
                    logger.LogInformation("Migrating database");
                    logger.LogInformation($"Pending migrations:");
                    foreach (var migration in migrations)
                    {
                        logger.LogInformation(migration);
                    }
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Migrated successfully");
                }
                else
                {
                    logger.LogInformation($"No pending migrations");
                }

                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var domain = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN");
                var secret = Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET");

                await botClient.SetWebhook(
                    url: $"https://{domain}/api/telegram/webhook",
                    secretToken: secret,
                    cancellationToken: CancellationToken.None);

            }

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => "OK");
            app.Run();
        }
    }
}
