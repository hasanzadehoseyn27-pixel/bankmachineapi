using BankeKhodroBot.Options;
using BankeKhodroBot.TelegramApi;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace BankeKhodroBot.Handlers;

public class PrivateFormHandler
{
    private readonly ITgSender _tg;
    private readonly BotOptions _opts;

    public PrivateFormHandler(ITgSender tg, IOptions<BotOptions> opts)
    {
        _tg = tg;
        _opts = opts.Value;
    }

    public async Task Handle(Message m, CancellationToken ct)
    {
        var text = (m.Text ?? string.Empty).Trim();

        if (string.Equals(text, "/post", StringComparison.OrdinalIgnoreCase))
        {
            var kb = new InlineKeyboardMarkupDto(new[]
            {
                new []
                {
                    new InlineKeyboardButtonDto(
                        "📝 باز کردن فرم آگهی",
                        WebApp: new BankeKhodroBot.TelegramApi.WebAppInfo(_opts.WebAppUrl) // ← نوعِ درست برای WebAppInfo
                    )
                }
            });

            await _tg.SendText(
                chatId: m.Chat.Id,
                text: "برای ثبت آگهی روی دکمه زیر بزن:",
                parseMode: "HTML",
                replyMarkup: kb,
                ct: ct
            );
            return;
        }

        if (string.Equals(text, "/start", StringComparison.OrdinalIgnoreCase))
        {
            await _tg.SendText(
                chatId: m.Chat.Id,
                text: "سلام! برای ثبت آگهی «/post» را بزن و فرم را پر کن.",
                ct: ct
            );
        }
    }
}
