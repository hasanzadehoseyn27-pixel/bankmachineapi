using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankeKhodroBot.TelegramApi;

public interface ITgSender
{
    Task SendText(long chatId, string text, string? parseMode = null, object? replyMarkup = null, CancellationToken ct = default);
    Task EditReplyMarkup(long chatId, int messageId, object? replyMarkup, CancellationToken ct = default);
    Task DeleteMessage(long chatId, int messageId, CancellationToken ct = default);
    Task AnswerCallback(string callbackQueryId, string? text = null, bool showAlert = false, CancellationToken ct = default);
    Task DeleteWebhook(bool dropPendingUpdates = true, CancellationToken ct = default);
    Task SendAlbum(long chatId, IEnumerable<StreamPhoto> photos, string? caption = null, string? parseMode = null, CancellationToken ct = default);
}

// === DTOs for keyboards ===

public record InlineKeyboardButtonDto(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("callback_data")] string? CallbackData = null,
    [property: JsonPropertyName("url")] string? Url = null,
    [property: JsonPropertyName("web_app")] WebAppInfo? WebApp = null // NEW
);

public record WebAppInfo([property: JsonPropertyName("url")] string Url);

public record InlineKeyboardMarkupDto(
    [property: JsonPropertyName("inline_keyboard")] InlineKeyboardButtonDto[][] InlineKeyboard
);

// === for album upload ===
public record StreamPhoto(string Name, Stream Content);

public class TgHttp : ITgSender
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public TgHttp(HttpClient http)
    {
        _http = http;
        _json = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null
        };
    }

    private async Task PostJson(string method, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _json);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(method, body, ct);
        var content = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Telegram API '{method}' failed ({(int)resp.StatusCode}): {content}");
    }

    public Task SendText(long chatId, string text, string? parseMode = null, object? replyMarkup = null, CancellationToken ct = default)
        => PostJson("sendMessage", new { chat_id = chatId, text, parse_mode = parseMode, reply_markup = replyMarkup }, ct);

    public Task EditReplyMarkup(long chatId, int messageId, object? replyMarkup, CancellationToken ct = default)
        => PostJson("editMessageReplyMarkup", new { chat_id = chatId, message_id = messageId, reply_markup = replyMarkup }, ct);

    public Task DeleteMessage(long chatId, int messageId, CancellationToken ct = default)
        => PostJson("deleteMessage", new { chat_id = chatId, message_id = messageId }, ct);

    public Task AnswerCallback(string callbackQueryId, string? text = null, bool showAlert = false, CancellationToken ct = default)
        => PostJson("answerCallbackQuery", new { callback_query_id = callbackQueryId, text, show_alert = showAlert }, ct);

    public Task DeleteWebhook(bool dropPendingUpdates = true, CancellationToken ct = default)
        => PostJson("deleteWebhook", new { drop_pending_updates = dropPendingUpdates }, ct);

    public async Task SendAlbum(long chatId, IEnumerable<StreamPhoto> photos, string? caption = null, string? parseMode = null, CancellationToken ct = default)
    {
        // sendMediaGroup with multipart attachments (attach://fileX)
        using var form = new MultipartFormDataContent();
        var media = new List<object>();
        int i = 0;
        foreach (var p in photos)
        {
            var field = $"photo{i}";
            form.Add(new StreamContent(p.Content), field, p.Name);
            media.Add(new
            {
                type = "photo",
                media = $"attach://{field}"
            });
            i++;
        }

        form.Add(new StringContent(chatId.ToString()), "chat_id");
        form.Add(new StringContent(JsonSerializer.Serialize(media, _json), Encoding.UTF8, "application/json"), "media");

        using var resp = await _http.PostAsync("sendMediaGroup", form, ct);
        var content = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Telegram API 'sendMediaGroup' failed ({(int)resp.StatusCode}): {content}");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            // optional follow-up caption message
            await PostJson("sendMessage", new { chat_id = chatId, text = caption, parse_mode = parseMode }, ct);
        }
    }
}
