using Telegram.Bot.Types;
using TelegramGpt.Services;

namespace TelegramGpt.Filters;

/// <summary>
/// Filters out updates from unauthorized Telegram users.
/// </summary>
public sealed class AuthorizedUserFilter(IUserAuthorizationService authorizationService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var update = context.Arguments.OfType<Update>().FirstOrDefault();
        var userId = update?.Message?.From?.Id;

        if (userId is null || !await authorizationService.IsAuthorizedAsync(userId.Value, context.HttpContext.RequestAborted))
        {
            return TypedResults.Ok();
        }

        return await next(context);
    }
}
