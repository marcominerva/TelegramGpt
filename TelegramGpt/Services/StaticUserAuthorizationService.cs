namespace TelegramGpt.Services;

/// <summary>
/// A test implementation that authorizes users against a hardcoded list of Telegram user IDs.
/// </summary>
public sealed class StaticUserAuthorizationService : IUserAuthorizationService
{
    private static readonly HashSet<long> authorizedUserIds = [792317159];

    public Task<bool> IsAuthorizedAsync(long telegramUserId, CancellationToken cancellationToken = default)
        => Task.FromResult(authorizedUserIds.Contains(telegramUserId));
}
