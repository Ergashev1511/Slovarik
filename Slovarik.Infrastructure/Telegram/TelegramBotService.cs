using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slovarik.Application.Interfaces;
using Slovarik.Infrastructure.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Slovarik.Infrastructure.Telegram;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<TelegramBotSettings> _settings;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(
        ITelegramBotClient botClient,
        IServiceScopeFactory scopeFactory,
        IOptions<TelegramBotSettings> settings,
        ILogger<TelegramBotService> logger)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Telegram bot ishga tushdi: @{Username}", me.Username);

        await _botClient.ReceiveAsync(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message?.Text is not { } text)
            return;

        if (text.StartsWith("/start"))
        {
            await HandleStartCommandAsync(botClient, update.Message, ct);
        }
    }

    private async Task HandleStartCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var from = message.From;
        if (from is null)
            return;

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var fullName = $"{from.FirstName} {from.LastName}".Trim();
        var user = await userService.GetOrCreateUserAsync(from.Id, from.Username, fullName, ct);

        var miniAppUrl = $"{_settings.Value.MiniAppUrl}?userId={user.Id}";
        var keyboard = new InlineKeyboardMarkup(
            new InlineKeyboardButton
            {
                Text = "Slovarik Mini App",
                WebApp = new WebAppInfo { Url = miniAppUrl }
            });

        await botClient.SendMessage(
            message.Chat.Id,
            "Xush kelibsiz! \n\nSlovarik — o'zbek-rus lug'at va test ilovasi.\nMini App ni ochish uchun tugmani bosing:",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Telegram bot xatosi");
        return Task.CompletedTask;
    }
}
