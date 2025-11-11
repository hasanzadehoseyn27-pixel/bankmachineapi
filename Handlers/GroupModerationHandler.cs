using BankeKhodroBot.TelegramApi;
using BankeKhodroBot.Utils;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BankeKhodroBot.Handlers;

public class GroupModerationHandler
{
    private readonly ITgSender _tg;
    private readonly ILogger<GroupModerationHandler> _log;

    public GroupModerationHandler(ITgSender tg, ILogger<GroupModerationHandler> log)
    { _tg = tg; _log = log; }

    public async Task Handle(Message m, CancellationToken ct)
    {
        if (m.From?.IsBot == true) return;

        if (m.Type == MessageType.Text)
        {
            var t = (m.Text ?? "").Trim();
            if (t.StartsWith("/ids", StringComparison.OrdinalIgnoreCase))
            { await _tg.SendText(m.Chat.Id, $"From.Id = {m.From!.Id}\nChat.Id = {m.Chat.Id}", ct: ct); return; }
        }

        bool nonText = m.Type != MessageType.Text;
        bool badText = m.Type == MessageType.Text && TextGuards.ContainsContactOrLink(m.Text);

        if (nonText || badText)
        {
            try { await _tg.DeleteMessage(m.Chat.Id, m.MessageId, ct); }
            catch (Exception ex) { _log.LogWarning(ex, "Cannot delete message {Id}", m.MessageId); }
        }
    }
}
