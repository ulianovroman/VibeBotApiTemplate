# BotApiTemplate

## Local build requirements

Project targets **.NET 8** (`net8.0`), so install .NET SDK 8.x before running build locally.

### Ubuntu 24.04

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### Verify installation

```bash
dotnet --info
dotnet restore
dotnet build -c Release
```

## Docker build

Repository already uses a multi-stage Dockerfile with `mcr.microsoft.com/dotnet/sdk:8.0` for build and `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime, so host machine does not need SDK when building image in Docker.

```bash
docker build -t bot-api-template .
```

## Environment variables

The application reads the following variables from environment:

| Variable | Required | Description |
| --- | --- | --- |
| `DATABASE_URL` | Yes | PostgreSQL connection URL in the format `postgres://<user>:<password>@<host>:<port>/<db_name>`. Used at startup to build EF Core/Npgsql connection string. If missing, app throws `InvalidOperationException` and does not start. |
| `TELEGRAM_BOT_TOKEN` | Yes (for runtime) | Telegram bot token. Used to create `ITelegramBotClient` and make Telegram API calls (including webhook registration). |
| `RAILWAY_PUBLIC_DOMAIN` | Yes | Public domain used to compose webhook URL: `https://<RAILWAY_PUBLIC_DOMAIN>/api/telegram/webhook`. |
| `TELEGRAM_WEBHOOK_SECRET` | Yes | Secret token for Telegram webhook security. Passed when registering webhook and validated for incoming webhook requests via `X-Telegram-Bot-Api-Secret-Token` header. |
| `GPT_API_KEY` | No | API key for GPT provider (template for `GptService`). Current `GptService` is a scaffold and does not call the model yet. |
| `GPT_MODEL` | No | Model name for GPT provider (template for `GptService`), default in config: `gpt-4o-mini`. |

### Notes

- `GptService` is registered in DI as a template (`IGptService`) and reads the `Gpt` configuration section (`ApiKey`, `Model`), but currently returns an empty result by design.
- Quartz.NET is connected through DI (`AddQuartz` + hosted service) with default in-memory store, which is sufficient for template bot scenarios without scheduler persistence.
- `TELEGRAM_BOT_TOKEN`, `RAILWAY_PUBLIC_DOMAIN`, and `TELEGRAM_WEBHOOK_SECRET` should be considered mandatory for normal app operation.
- Even though `TELEGRAM_BOT_TOKEN` is checked as optional during DI registration, startup flow later resolves `ITelegramBotClient` unconditionally; without token runtime startup will fail when initializing webhook.
