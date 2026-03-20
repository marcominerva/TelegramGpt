using Microsoft.Extensions.Options;
using TelegramGpt.Settings;

namespace TelegramGpt.Filters;

/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
public sealed class ValidateTelegramBotFilter(IOptions<BotSettings> botSettingsOptions) : IEndpointFilter
{
    private const string TelegramSecretTokenHeaderName = "X-Telegram-Bot-Api-Secret-Token";

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var isSecretTokenProvided = context.HttpContext.Request.Headers.TryGetValue(TelegramSecretTokenHeaderName, out var headerSecretToken);

        if (!isSecretTokenProvided || headerSecretToken.ToString() != botSettingsOptions.Value.SecretToken)
        {
            return ValueTask.FromResult<object?>(TypedResults.Ok());
        }

        return next(context);
    }
}