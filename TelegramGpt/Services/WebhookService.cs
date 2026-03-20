using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramGpt.Settings;

namespace TelegramGpt.Services;

public class WebhookService(IServiceScopeFactory serviceScopeFactory, IOptions<BotSettings> botOptions) : IHostedService
{
    private readonly BotSettings botSettings = botOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Configure custom endpoint per Telegram API recommendations:
        // https://core.telegram.org/bots/api#setwebhook
        // If you'd like to make sure that the webhook was set by you, you can specify secret data
        // in the parameter secret_token. If specified, the request will contain a header
        // "X-Telegram-Bot-Api-Secret-Token" with the secret token as content.
        await botClient.SetWebhook(url: botSettings.WebhookUri, secretToken: botSettings.SecretToken, allowedUpdates: [], cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook on app shutdown
        await botClient.DeleteWebhook(cancellationToken: cancellationToken);
    }
}
