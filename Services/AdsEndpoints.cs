using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BankeKhodroBot.Models;
using BankeKhodroBot.Services;
using BankeKhodroBot.TelegramApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using BankeKhodroBot.Options;

namespace BankeKhodroBot.Services;

public static class AdsEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/ads/submit", async (
            HttpRequest req,
            IRuntimeConfig rt,
            IPendingAdStore pending,
            ITgSender tg,
            IOptions<BotOptions> botOpts,
            CancellationToken ct) =>
        {
            // multipart form expected
            if (!req.HasFormContentType)
                return Results.BadRequest("multipart/form-data expected.");

            var form = await req.ReadFormAsync(ct);

            // validate Telegram init data
            var initData = form["tgInitData"].ToString();
            if (string.IsNullOrWhiteSpace(initData))
                return Results.BadRequest("Missing tgInitData.");

            if (!ValidateTelegramInitData(initData, botOpts.Value.Token, out var user))
                return Results.Unauthorized();

            // fields
            var ad = new CarAd
            {
                UserId = user.Id,
                SubmitterName = user.DisplayName, // نمایش نام کاربر به‌جای آی‌دی
                RequestType = Enum.TryParse<RequestType>(form["requestType"], out var r) ? r : RequestType.Sell,
                CarName = form["carName"],
                Year = form["year"],
                Color = form["color"],
                Mileage = form["mileage"],
                BodyCondition = form["body"],
                ChassisCondition = form["chassis"],
                TireCondition = form["tires"],       // ✅ وضعیت لاستیک
                Mechanical = form["mechanical"],
                Gearbox = form["gearbox"],
                Price = form["price"],
                Extra = form["extra"]
            };

            // send album to admin (if any photo)
            var files = form.Files;
            var streams = new List<StreamPhoto>();
            foreach (var f in files)
            {
                if (f.Length == 0) continue;
                var ms = new MemoryStream();
                await f.CopyToAsync(ms, ct);
                ms.Position = 0;
                streams.Add(new StreamPhoto(f.FileName, ms));
            }

            var adId = Guid.NewGuid().ToString("N");
            pending.PendingAds[adId] = ad;

            var adminText = ad.ToPrettyForAdmin(); // استایل شکیل برای ادمین
            var kb = new
            {
                inline_keyboard = new[]
                {
                    new[] {
                        new { text = "✅ تایید", callback_data = $"approve:{adId}" },
                        new { text = "❌ رد",   callback_data = $"reject:{adId}" }
                    }
                }
            };

            try
            {
                if (rt.AdminChatId == 0) return Results.BadRequest("AdminChatId not set.");

                if (streams.Count > 0)
                    await tg.SendAlbum(rt.AdminChatId, streams, ct: ct);

                await tg.SendText(rt.AdminChatId, adminText, "HTML", kb, ct);

                // notify user (optional)
                await tg.SendText(ad.UserId, "درخواست شما برای ادمین ارسال شد و در انتظار تایید است.", ct: ct);
                return Results.Ok(new { ok = true });
            }
            finally
            {
                // dispose memory streams
                foreach (var s in streams) s.Content.Dispose();
            }
        });
    }

    // ===== Telegram WebApp initData validation =====
    private static bool ValidateTelegramInitData(string initData, string botToken, out (long Id, string DisplayName) user)
    {
        user = default;

        var pairs = initData.Split('&', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Split('=', 2))
                            .ToDictionary(a => a[0], a => Uri.UnescapeDataString(a.Length > 1 ? a[1] : ""));

        if (!pairs.TryGetValue("hash", out var hash)) return false;

        var dataCheckString = string.Join("\n",
            pairs.Where(kv => kv.Key != "hash")
                 .OrderBy(kv => kv.Key)
                 .Select(kv => $"{kv.Key}={kv.Value}"));

        // secret_key = HMAC_SHA256("WebAppData", bot_token)
        var secret = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"),
                                         Encoding.UTF8.GetBytes(botToken));
        var hmac = HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(dataCheckString));
        var hex = string.Concat(hmac.Select(b => b.ToString("x2")));

        if (!hex.Equals(hash, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!pairs.TryGetValue("user", out var userJson)) return false;

        // ❌ مشکل قبلی: سازنده بدون پارامتر نداشت → با مقادیر پیش‌فرض مقداردهی کن
        var u = JsonSerializer.Deserialize<TgUser>(userJson) ?? new TgUser(0, null, null, null);

        var display = !string.IsNullOrWhiteSpace(u.username) ? $"@{u.username}" :
                      $"{u.first_name} {u.last_name}".Trim();

        if (string.IsNullOrWhiteSpace(display)) display = u.id.ToString();
        user = (u.id, display);
        return true;
    }

    private record TgUser(long id, string? username, string? first_name, string? last_name);
}
