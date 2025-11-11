using BankeKhodroBot.Models;
using BankeKhodroBot.Options;
using BankeKhodroBot.Services;
using BankeKhodroBot.TelegramApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace BankeKhodroBot.Handlers;

public class AdminCallbackHandler
{
    private readonly ITgSender _tg;
    private readonly IPendingAdStore _pending;
    private readonly IRuntimeConfig _rt;
    private readonly BotOptions _opts;
    private readonly ILogger<AdminCallbackHandler> _log;

    public AdminCallbackHandler(ITgSender tg, IPendingAdStore pending, IRuntimeConfig rt, IOptions<BotOptions> opts, ILogger<AdminCallbackHandler> log)
    {
        _tg = tg; _pending = pending; _rt = rt; _opts = opts.Value; _log = log;
    }

    public async Task Handle(CallbackQuery cq, CancellationToken ct)
    {
        if (cq.Data is null) return;

        async Task RemoveMarkup()
        {
            if (cq.Message != null)
            {
                try { await _tg.EditReplyMarkup(cq.Message.Chat.Id, cq.Message.MessageId, null, ct); } catch { }
            }
        }

        if (cq.Data.StartsWith("approve:", StringComparison.Ordinal))
        {
            var id = cq.Data["approve:".Length..];
            if (_pending.PendingAds.TryRemove(id, out var ad))
            {
                try
                {
                    if (_rt.GroupChatId == 0) throw new Exception("GroupChatId is 0");
                    var text = ad.ToPrettyForGroup(_opts.ContactPhone, _opts.ContactName);
                    await _tg.SendText(_rt.GroupChatId, text, "HTML", null, ct);

                    await RemoveMarkup();
                    if (cq.Message != null) await _tg.SendText(cq.Message.Chat.Id, "✅ در گروه منتشر شد.", ct: ct);
                    await _tg.SendText(ad.UserId, "آگهی‌ات تایید و منتشر شد ✅", ct: ct);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Publish failed");
                    await _tg.AnswerCallback(cq.Id, "ارسال به گروه ناموفق بود. /setgroup را تنظیم کن.", true, ct);
                }
            }
            else
            {
                await _tg.AnswerCallback(cq.Id, "یافت نشد / رسیدگی شده.", true, ct);
            }
            return;
        }

        if (cq.Data.StartsWith("reject:", StringComparison.Ordinal))
        {
            var id = cq.Data["reject:".Length..];
            if (_pending.PendingAds.TryRemove(id, out var ad))
            {
                await RemoveMarkup();
                if (cq.Message != null) await _tg.SendText(cq.Message.Chat.Id, "❌ رد شد.", ct: ct);
                try { await _tg.SendText(ad.UserId, "متاسفانه آگهی‌ات رد شد.", ct: ct); } catch { }
            }
            else
            {
                await _tg.AnswerCallback(cq.Id, "یافت نشد / رسیدگی شده.", true, ct);
            }
        }
    }
}
