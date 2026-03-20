# Agent Instructions and Project Context (VibeBotApi)

This file is the primary onboarding source for agents working in this repository.

## Non-negotiable repository rules

- Write code comments in English only.
- Do not add comments in any other language.
- Register Quartz jobs through centralized registration extensions (for example, `QuartzRegistrationExtensions`) rather than inline scheduler configuration.

---

## 1) Project in 30 seconds

`VibeBotApi` is an ASP.NET Core (.NET 8) backend template for Telegram bots that use webhooks.

Core capabilities already included:
- Telegram webhook endpoint.
- Update processing pipeline (Chain of Responsibility).
- PostgreSQL persistence via EF Core + migrations.
- Optional OpenAI/GPT service abstraction.
- Quartz background jobs with attribute-based schedule registration.

---

## 2) Architecture map

```text
Telegram -> POST /api/telegram/webhook
         -> TelegramWebhookController
         -> UpdateProcessor
         -> SetUserHandler -> LoggingHandler -> StartCommandHandler
                                        |
                                        +-> BotContext (PostgreSQL)

Startup:
Program.cs -> StartupHelper.RegisterDependencies() -> StartupHelper.Init()
```

### Processing flow details
1. Telegram sends an update to `/api/telegram/webhook`.
2. `TelegramWebhookController` validates `X-Telegram-Bot-Api-Secret-Token`.
3. Update is forwarded to `UpdateProcessor`.
4. Pipeline handlers run in DI registration order.
5. `SetUserHandler` upserts user info and fills `UpdateContext.User`.
6. `LoggingHandler` stores message text snippets in `MessageLogs`.
7. `StartCommandHandler` handles `/start` and checks `UserPermissions`.

---

## 3) Key files and ownership map

- `Program.cs`  
  Minimal entrypoint.

- `StartupHelper.cs`  
  Main composition root: env validation, DI registration, EF setup, webhook setup, startup migrations.

- `Controllers/TelegramWebhookController.cs`  
  Incoming Telegram webhook + webhook settings endpoint.

- `Controllers/BotActionsController.cs`  
  Internal endpoint to send messages via bot API.

- `UpdateChainOfResponsibility/*`  
  Handler pipeline contracts and implementations.

- `Storage/*` + `Migrations/*`  
  Database entities, `BotContext`, and schema history.

- `Jobs/*`  
  Quartz job infrastructure and example scheduled job.

- `Service/*`  
  GPT service abstraction and implementation.

---

## 4) Environment variables

### Required
- `DATABASE_URL`
- `TELEGRAM_BOT_TOKEN`
- `RAILWAY_PUBLIC_DOMAIN`
- `TELEGRAM_WEBHOOK_SECRET`
- `VIBE_BOT_API_SECRET`

### Optional
- `GPT_API_KEY`
- `GPT_MODEL` (defaults to `gpt-4o-mini`)

### Notes
- Startup fails early with explicit messages if required variables are missing.
- `DATABASE_URL` format must be:
  `postgres://<user>:<password>@<host>:<port>/<db_name>`
- Webhook URL is configured as:
  `https://<RAILWAY_PUBLIC_DOMAIN>/api/telegram/webhook`

---

## 5) API quick reference

- `POST /api/telegram/webhook`
  - Purpose: Telegram update intake.
  - Auth: `X-Telegram-Bot-Api-Secret-Token` header.

- `POST /api/telegram/webhook/settings`
  - Purpose: Read current webhook settings from Telegram.
  - Auth: `vibeBotApiSecret` in JSON body.

- `POST /api/bot/actions/send-message`
  - Purpose: Send message as bot.
  - Auth: `vibeBotApiSecret` in JSON body.

---

## 6) Agent playbook (common tasks)

### Add a new update handler
1. Create a class in `UpdateChainOfResponsibility` implementing `IUpdateHandler`.
2. Use `UpdateContext` for passing values between handlers.
3. Register the handler in `UpdateChainOfResponsibilityConfigurator` in the intended order.
4. Add migration if persistence model changed.

### Add a new API endpoint
1. Add/update controller in `Controllers`.
2. Reuse existing secret-based protection for internal endpoints.
3. Add DTOs under `Dto/`.
4. Update documentation when behavior is public or integration-facing.

### Add a new Quartz job
1. Create a class in `Jobs/` that implements `IJob`.
2. Add `[QuartzSchedule("...")]` attribute.
3. Do not add inline scheduler configuration elsewhere.

### Extend GPT behavior
1. Modify prompt/response logic in `GptService`.
2. If logic grows, extract domain service(s) on top of `IGptService`.

---

## 7) Risk awareness before changing code

- Secrets are currently validated directly in controllers; avoid duplicating this pattern in many places.
- `LoggingHandler` writes to DB per text update; high-throughput bots may need async queue/batching.
- `/start` response text is hardcoded; future localization should move texts to dedicated templates/resources.

---

## 8) Pull request checklist for agents

- Build passes (`dotnet build -c Release`) in a proper .NET 8 environment.
- EF model changes include migration.
- Handler ordering verified when pipeline changed.
- New Quartz job uses centralized/attribute registration only.
- Documentation updated when public behavior changed.
