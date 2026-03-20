# VibeBotApi

A beginner-friendly ASP.NET Core (.NET 8) template for building Telegram bots with webhooks, PostgreSQL, and optional GPT integration.

---

## Who this is for

This repository is made for:
- beginner and intermediate developers who want to ship a Telegram bot backend quickly;
- "vibe coders" who want a safe structure and practical defaults;
- teams who need a clean starting point instead of building infra from scratch.

If you can run basic terminal commands, you can start from this template.

---

## What you get out of the box

- Webhook endpoint for Telegram updates.
- Update pipeline with Chain of Responsibility handlers.
- PostgreSQL + EF Core + ready migration baseline.
- Optional GPT service interface and implementation scaffold.
- Quartz jobs with automatic attribute-based registration.
- Dockerfile for deployment.

---

## Mental model (read this first)

### Runtime flow

```text
Telegram update
  -> /api/telegram/webhook
  -> TelegramWebhookController
  -> UpdateProcessor
  -> SetUserHandler -> LoggingHandler -> StartCommandHandler
```

### Startup flow

```text
Program.cs
  -> StartupHelper.RegisterDependencies()
  -> StartupHelper.Init()
```

What happens on startup:
1. Required environment variables are validated.
2. DbContext is configured from `DATABASE_URL`.
3. Telegram client + services are registered.
4. Quartz jobs are discovered by attribute.
5. Pending EF migrations are applied.
6. Telegram webhook is configured.

---

## Project map (where to look)

- `Program.cs` — minimal app entrypoint.
- `StartupHelper.cs` — startup logic and dependency registration.
- `Controllers/` — HTTP endpoints.
- `UpdateChainOfResponsibility/` — update handler pipeline.
- `Storage/` — EF Core entities and `BotContext`.
- `Migrations/` — schema migration files.
- `Jobs/` — Quartz schedule attribute + jobs.
- `Service/` — GPT integration layer.
- `AGENTS.md` — deep onboarding and contributor instructions.

---

## Prerequisites

- .NET SDK 8.x
- PostgreSQL database
- Telegram bot token (from BotFather)

### Install .NET 8 on Ubuntu 24.04

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

Verify:

```bash
dotnet --info
```

---

## Required environment variables

Set these before running the app:

| Variable | Required | Description |
| --- | --- | --- |
| `DATABASE_URL` | Yes | Postgres URL: `postgres://<user>:<password>@<host>:<port>/<db_name>` |
| `TELEGRAM_BOT_TOKEN` | Yes | Telegram bot token |
| `RAILWAY_PUBLIC_DOMAIN` | Yes | Public domain used to build webhook URL |
| `TELEGRAM_WEBHOOK_SECRET` | Yes | Secret Telegram sends in `X-Telegram-Bot-Api-Secret-Token` |
| `VIBE_BOT_API_SECRET` | Yes | Secret for internal API endpoints |
| `GPT_API_KEY` | No | API key for GPT provider |
| `GPT_MODEL` | No | GPT model name, default `gpt-4o-mini` |

---

## Local run (step-by-step)

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build -c Release
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

4. Open Swagger UI (default):
   - `http://localhost:5000/swagger` (or the port shown in logs)

---

## Docker build

```bash
docker build -t bot-api-template .
```

---

## API endpoints you will actually use

### 1) Telegram webhook receiver
- `POST /api/telegram/webhook`
- Auth: header `X-Telegram-Bot-Api-Secret-Token`
- Purpose: receive Telegram updates

### 2) Check webhook settings
- `POST /api/telegram/webhook/settings`
- Auth: JSON body field `vibeBotApiSecret`
- Purpose: read current webhook info from Telegram API

### 3) Send message manually
- `POST /api/bot/actions/send-message`
- Auth: JSON body field `vibeBotApiSecret`
- Purpose: send a message via bot (useful for tests/admin actions)

Example request:

```bash
curl -X POST http://localhost:5000/api/bot/actions/send-message \
  -H 'Content-Type: application/json' \
  -d '{
    "vibeBotApiSecret": "your_secret",
    "chatId": 123456789,
    "text": "Hello from VibeBotApi"
  }'
```

---

## How to extend safely (for inexperienced contributors)

### Add a new update behavior
- Create a new handler in `UpdateChainOfResponsibility/` implementing `IUpdateHandler`.
- Register it in `UpdateChainOfResponsibilityConfigurator`.
- Be careful with order: handlers run in registration order.

### Add new database fields
- Update entity in `Storage/`.
- Create migration:
  ```bash
  dotnet ef migrations add <Name>
  ```
- Apply migration:
  ```bash
  dotnet ef database update
  ```

### Add scheduled background task
- Add class in `Jobs/` implementing `IJob`.
- Add `[QuartzSchedule("CRON", TimeZoneId = "UTC")]`.
- Do not configure Quartz inline in random files.

### Add GPT-based logic
- Use `IGptService` and update `GptService` prompt structure.
- Keep business logic outside transport details when possible.

---

## Common mistakes to avoid

- Missing required env vars (startup will fail by design).
- Wrong `DATABASE_URL` format.
- Forgetting to register new handlers in configurator.
- Changing pipeline order accidentally.
- Adding Quartz job without `QuartzSchedule` attribute.

---

## Contributor docs

- For agent-focused rules and deeper architecture context, read `AGENTS.md`.

---

## License

MIT. See [LICENSE](./LICENSE).
