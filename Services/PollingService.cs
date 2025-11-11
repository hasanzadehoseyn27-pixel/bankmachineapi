using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace BankeKhodroBot.Services;

public class PollingService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUpdateHandler _handler;
    private readonly ILogger<PollingService> _log;

    public PollingService(ITelegramBotClient bot, IUpdateHandler handler, ILogger<PollingService> log)
    {
        _bot = bot; _handler = handler; _log = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
        _log.LogInformation("Long-polling started.");
        _bot.StartReceiving(_handler, opts, cancellationToken: stoppingToken);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        _log.LogInformation("Stopping polling…");
        await base.StopAsync(ct);
    }
}
