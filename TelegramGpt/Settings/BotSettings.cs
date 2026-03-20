namespace TelegramGpt.Settings;

public class BotSettings
{
    public required string BotToken { get; init; }

    public required string WebhookUri { get; init; }

    public required string SecretToken { get; init; }
}
