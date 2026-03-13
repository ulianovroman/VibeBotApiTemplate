# BotApiTemplate

## What this repository is

`BotApiTemplate` is an **ASP.NET Core (.NET 8)** backend template for building Telegram bots with webhooks.

You can use it as a starting point when you want to launch a bot quickly with core layers already prepared:

- HTTP API endpoint for receiving Telegram webhook updates;
- update processing pipeline based on Chain of Responsibility;
- PostgreSQL integration via EF Core + migrations;
- AI/GPT service infrastructure (interface + ready-to-edit direct API call example);
- Dockerized deployment flow (for Railway, Render, Fly.io, VPS, etc.).

## What's already implemented

- **Telegram webhook controller** (`Controllers/TelegramWebhookController.cs`) for receiving and validating incoming updates.
- **Update processing pipeline** (`UpdateChainOfResponsibility/*`) including:
  - user identification,
  - `/start` command handling,
  - logging and extensible handler architecture.
- **Data storage layer** (`Storage/*`) including:
  - `BotContext` (EF Core `DbContext`),
  - user and message-log entities,
  - ready-to-use PostgreSQL migrations.
- **Service layer** (`Service/*`) including:
  - bot phrases,
  - Telegram API extensions,
  - `IGptService` and `GptService` with direct GPT API call template.
- **Runtime infrastructure** including:
  - dependency injection and middleware setup in `Program.cs`,
  - multi-stage `Dockerfile`,
  - example app settings in `appsettings*.json`.

## Typical use cases

This template is a good fit for:

- Telegram bot MVPs (FAQ bot, support bot, personal assistant);
- internal team bots (notifications and simple workflows);
- pet projects and educational tasks around ASP.NET Core + Telegram Bot API;
- a foundation for LLM-powered bots (by implementing real logic inside `GptService`).

If you want to bootstrap a bot backend fast and avoid building the base architecture from scratch, this repository is intended for exactly that.

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
| `GPT_API_KEY` | No | API key for GPT provider. If configured, `GptService` performs a direct GPT API call. |
| `GPT_MODEL` | No | Model name for GPT provider for direct API calls in `GptService`, default: `gpt-4o-mini`. |

### Notes

- `GptService` is registered in DI (`IGptService`), reads the `Gpt` configuration section (`ApiKey`, `Model`), and contains a direct GPT API call with example system/user prompt templates you can replace with your own prompts.
- Quartz.NET is connected through DI (`AddQuartz` + hosted service) with default in-memory store and automatic job discovery by attribute: add a class in `Jobs` implementing `IJob` and annotate it with `QuartzSchedule` to register schedule automatically. Includes `DailySixPmUtcLogJob` as an example (18:00 UTC daily).
- `TELEGRAM_BOT_TOKEN`, `RAILWAY_PUBLIC_DOMAIN`, and `TELEGRAM_WEBHOOK_SECRET` should be considered mandatory for normal app operation.
- Even though `TELEGRAM_BOT_TOKEN` is checked as optional during DI registration, startup flow later resolves `ITelegramBotClient` unconditionally; without token runtime startup will fail when initializing webhook.

---

## Русская версия (кратко)

`BotApiTemplate` — шаблон backend-приложения на **ASP.NET Core (.NET 8)** для Telegram-бота через webhook.

Что есть в репозитории:

- контроллер webhook Telegram;
- цепочка обработчиков апдейтов (включая `/start` и логирование);
- слой хранения с `BotContext` (EF Core + PostgreSQL миграции);
- сервисный слой с `IGptService`/`GptService` и примером прямого GPT API-вызова;
- инфраструктура запуска и Docker-конфигурация.

Для чего использовать:

- быстрый старт MVP Telegram-бота;
- внутренние боты для команды;
- учебные/пет-проекты;
- база для последующей интеграции LLM.
