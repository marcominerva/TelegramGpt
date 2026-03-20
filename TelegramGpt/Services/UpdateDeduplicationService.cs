namespace TelegramGpt.Services;

/// <summary>
/// Detects duplicate Telegram updates using sequential ID comparison
/// with time-based expiration to handle Telegram's random ID reset after inactivity.
/// </summary>
public sealed class UpdateDeduplicationService(TimeProvider timeProvider)
{
    private static readonly TimeSpan expirationThreshold = TimeSpan.FromDays(5);

    private readonly Lock @lock = new();
    private int lastUpdateId;
    private DateTimeOffset lastUpdateTime;

    /// <summary>
    /// Returns <c>true</c> if the update has already been processed.
    /// </summary>
    public bool IsDuplicate(int updateId)
    {
        lock (@lock)
        {
            var now = timeProvider.GetUtcNow();

            if (lastUpdateTime != default && now - lastUpdateTime < expirationThreshold && updateId <= lastUpdateId)
            {
                return true;
            }

            lastUpdateId = updateId;
            lastUpdateTime = now;

            return false;
        }
    }
}
