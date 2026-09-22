# TelegramGpt

TelegramGpt is a .NET 10 ASP.NET Core application that hosts a Telegram bot through a webhook. It provides the foundation for conversational automation while keeping Telegram request validation, user authorization, update deduplication, and message handling separated into dedicated components.

## How it works

At startup, `WebhookService` registers the configured public URL with Telegram and supplies a secret token. Telegram then sends updates to `POST /api/bot`; the webhook is removed when the application shuts down.

Incoming updates pass through two endpoint filters before being processed:

1. `ValidateTelegramBotFilter` verifies the `X-Telegram-Bot-Api-Secret-Token` header. Requests without the configured secret are acknowledged without being processed.
2. `AuthorizedUserFilter` obtains the Telegram user ID from the update and delegates access control to `IUserAuthorizationService`. The current `StaticUserAuthorizationService` implementation is intended for testing and uses a hardcoded allowlist.

Accepted updates are checked by `UpdateDeduplicationService` to prevent repeated processing. The current handler:

- replies to text messages by echoing their content;
- downloads the largest available version of received photos to the system temporary directory;
- recognizes location messages, with location handling still to be implemented;
- ignores unsupported or non-message updates.

The bot endpoint always acknowledges filtered, duplicate, or unsupported updates with HTTP 200 so Telegram does not retry them unnecessarily.

## Configuration

Configure the `BotSettings` section in `TelegramGpt/appsettings.local.json` or through another ASP.NET Core configuration provider:

```json
{
  "BotSettings": {
    "BotToken": "<telegram-bot-token>",
    "WebhookUri": "https://<public-host>/api/bot",
    "SecretToken": "<webhook-secret-token>"
  }
}
```

`appsettings.local.json` is optional and loaded with reload-on-change enabled. Do not commit real bot tokens or webhook secrets.

The webhook URI must be publicly reachable over HTTPS for Telegram to deliver updates.

## Run locally

The project requires the .NET 10 SDK. After providing valid configuration, run:

```bash
dotnet run --project TelegramGpt/TelegramGpt.csproj
```

## Main components

| Component | Responsibility |
| --- | --- |
| `Program.cs` | Configures dependency injection, the Telegram client, middleware, filters, and the webhook endpoint. |
| `WebhookService` | Registers and removes the Telegram webhook with the configured secret token. |
| `ValidateTelegramBotFilter` | Validates that webhook calls carry the expected Telegram secret header. |
| `AuthorizedUserFilter` | Rejects updates that do not belong to an authorized Telegram user. |
| `IUserAuthorizationService` | Defines the abstraction used to supply a different authorization strategy. |
| `StaticUserAuthorizationService` | Provides the current test-only, hardcoded allowlist implementation. |
| `UpdateDeduplicationService` | Suppresses duplicate update IDs while accounting for Telegram ID resets after inactivity. |
