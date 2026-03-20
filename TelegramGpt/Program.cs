using Microsoft.AspNetCore.Http.HttpResults;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramGpt.Filters;
using TelegramGpt.Services;
using TelegramGpt.Settings;
using TinyHelpers.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddSingleton(TimeProvider.System);

var botSettings = builder.Services.ConfigureAndGet<BotSettings>(builder.Configuration, nameof(BotSettings))!;

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, _) =>
    {
        var options = new TelegramBotClientOptions(botSettings.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddHostedService<WebhookService>();

builder.Services.AddSingleton<UpdateDeduplicationService>();
builder.Services.AddScoped<IUserAuthorizationService, StaticUserAuthorizationService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
});

app.MapPost("/api/bot", OnUpdate)
    .AddEndpointFilter<ValidateTelegramBotFilter>()
    .AddEndpointFilter<AuthorizedUserFilter>()
    .ExcludeFromDescription();

app.Run();

static async Task<Ok> OnUpdate(Update update, ITelegramBotClient botClient, UpdateDeduplicationService deduplicationService, CancellationToken cancellationToken)
{
    if (deduplicationService.IsDuplicate(update.Id))
    {
        return TypedResults.Ok();
    }

    if (update.Message is not Message message)
    {
        return TypedResults.Ok();
    }

    if (message.Type == MessageType.Text)
    {
        await botClient.SendChatAction(message.Chat.Id, ChatAction.Typing, cancellationToken: cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        await botClient.SendMessage(message.Chat.Id, $"You said: '{message.Text}'",
            /* replyParameters: update.Message.Id, */
            cancellationToken: cancellationToken);
    }
    else if (message.Type == MessageType.Photo)
    {
        await using var ms = new MemoryStream();
        var tgFile = await botClient.GetInfoAndDownloadFile(message.Photo!.Last().FileId, ms, cancellationToken: cancellationToken);

        var fileName = Path.GetFileName(tgFile.FilePath)!;
        await File.WriteAllBytesAsync(Path.Combine(Path.GetTempPath(), fileName), ms.ToArray(), cancellationToken);
    }
    else if (message.Type == MessageType.Location)
    {

    }

    return TypedResults.Ok();
}