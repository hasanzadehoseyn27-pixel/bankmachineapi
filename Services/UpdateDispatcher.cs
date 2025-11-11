using BankeKhodroBot.Handlers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BankeKhodroBot.Services;

public class UpdateDispatcher : IUpdateHandler
{
    private readonly PrivateFormHandler _private;
    private readonly GroupModerationHandler _group;
    private readonly AdminCallbackHandler _admin;
    private readonly ILogger<UpdateDispatcher> _log;

    public UpdateDispatcher(PrivateFormHandler privateHandler,
                            GroupModerationHandler groupHandler,
                            AdminCallbackHandler adminHandler,
                            ILogger<UpdateDispatcher> log)
    {
        _private = privateHandler;
        _group = groupHandler;
        _admin = adminHandler;
        _log = log;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message is { } m)
            {
                if (m.Chat.Type == ChatType.Private)
                {
                    await _private.Handle(m, ct);
                    return;
                }

                if (m.Chat.Type is ChatType.Group or ChatType.Supergroup)
                {
                    await _group.Handle(m, ct);
                    return;
                }
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } cq)
            {
                // فقط تایید/رد ادمین
                await _admin.Handle(cq, ct);
                return;
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Update handling failed.");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, HandleErrorSource src, CancellationToken ct)
    {
        _log.LogError(ex, "Polling error ({Src})", src);
        return Task.CompletedTask;
    }
}
