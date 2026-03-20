namespace TelegramGpt.Services;

/// <summary>
/// Determines whether a Telegram user is authorized to interact with the bot.
/// </summary>
public interface IUserAuthorizationService
{
    Task<bool> IsAuthorizedAsync(long telegramUserId, CancellationToken cancellationToken = default);
}
