using BankeKhodroBot.TelegramApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankeKhodroBot.Services;

public class StartupService : IHostedService
{
    private readonly ITgSender _tg;
    private readonly IRuntimeConfig _rt;
    private readonly ILogger<StartupService> _log;

    public StartupService(ITgSender tg, IRuntimeConfig rt, ILogger<StartupService> log)
    {
        _tg = tg; _rt = rt; _log = log;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Runtime IDs: Admin={Admin}, Group={Group}", _rt.AdminChatId, _rt.GroupChatId);

        try
        {
            await _tg.DeleteWebhook(true, cancellationToken);
            _log.LogInformation("Webhook cleared.");
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "DeleteWebhook failed.");
        }

        if (_rt.AdminChatId != 0)
        {
            try { await _tg.SendText(_rt.AdminChatId, $"✅ Bot is online\nAdmin={_rt.AdminChatId}\nGroup={_rt.GroupChatId}", ct: cancellationToken); }
            catch (Exception ex) { _log.LogWarning(ex, "Cannot DM admin."); }
        }

        if (_rt.GroupChatId != 0)
        {
            try { await _tg.SendText(_rt.GroupChatId, "🤖 Bot connected", ct: cancellationToken); }
            catch (Exception ex) { _log.LogWarning(ex, "Cannot post to group."); }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
