using System.Net;

namespace BankeKhodroBot.Models;

public enum RequestType
{
    Buy,       // خرید
    Sell,      // فروش
    CoSell     // فروش همکاری
}

public class CarAd
{
    public long UserId { get; set; }

    // برای نمایش نام کاربر کنار درخواست
    public string SubmitterName { get; set; } = "";

    public RequestType RequestType { get; set; }
    public string CarName { get; set; } = "";
    public string Year { get; set; } = "";
    public string Color { get; set; } = "";
    public string Mileage { get; set; } = "";
    public string BodyCondition { get; set; } = "";
    public string ChassisCondition { get; set; } = "";
    public string TireCondition { get; set; } = "";      // ✅ وضعیت لاستیک
    public string Mechanical { get; set; } = "";
    public string Gearbox { get; set; } = "";

    // فقط برای ادمین
    public List<string> PhotoFileIds { get; set; } = new();
    public string Price { get; set; } = "";
    public string Extra { get; set; } = "";

    static string E(string s) => WebUtility.HtmlEncode(s);

    static string Fa(RequestType r) => r switch
    {
        RequestType.Buy => "خرید",
        RequestType.Sell => "فروش",
        RequestType.CoSell => "فروش همکاری",
        _ => ""
    };

    const string SEP = "━━━━━━━━━━━━━━━━";

    // متن شکیل برای ادمین (با قیمت/توضیحات)
    public string ToPrettyForAdmin() =>
$@"درخواست انتشار آگهی 📬
👤 کاربر: {E(SubmitterName)}
{SEP}
🧾 نوع درخواست: {Fa(RequestType)}
{SEP}

🚘 نام خودرو: {E(CarName)}
{SEP}

🗓️ سال ساخت: {E(Year)}
{SEP}

🎨 رنگ: {E(Color)}
{SEP}

⏱️ کارکرد: {E(Mileage)}
{SEP}

🛡️ وضعیت بدنه: {E(BodyCondition)}
{SEP}

🧱 وضعیت شاسی‌ها: {E(ChassisCondition)}
{SEP}

🛞 وضعیت لاستیک: {E(TireCondition)}
{SEP}

🔧 وضعیت موتور و فنی: {E(Mechanical)}
{SEP}

⚙️ نوع گیربکس: {E(Gearbox)}
{SEP}

🔒 قیمت (فقط ادمین): {E(Price)}
{SEP}
🔒 سایر توضیحات (فقط ادمین):
{E(Extra)}";

    // متن شکیل برای انتشار در گروه (بدون قیمت/توضیحات)
    public string ToPrettyForGroup(string contactPhone, string contactName) =>
$@"مشخصات خودرو
{SEP}

🧾 نوع درخواست: {Fa(RequestType)}
{SEP}

🚘 نام خودرو: {E(CarName)}
{SEP}

🗓️ سال ساخت: {E(Year)}
{SEP}

🎨 رنگ: {E(Color)}
{SEP}

⏱️ کارکرد: {E(Mileage)}
{SEP}

🛡️ وضعیت بدنه: {E(BodyCondition)}
{SEP}

🧱 وضعیت شاسی‌ها: {E(ChassisCondition)}
{SEP}

🛞 وضعیت لاستیک: {E(TireCondition)}
{SEP}

🔧 وضعیت موتور و فنی: {E(Mechanical)}
{SEP}

⚙️ نوع گیربکس: {E(Gearbox)}
{SEP}

📞 شماره تماس: {E(contactPhone)} - {E(contactName)}";
}
