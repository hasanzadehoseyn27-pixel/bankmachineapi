namespace BankeKhodroBot.Models;

public enum Step
{
    None = 0,

    // فرم
    RequestType,    // دکمه‌ای: خرید / فروش / فروش همکاری
    CarName,        // نام خودرو
    Year,           // سال ساخت
    Color,          // رنگ
    Mileage,        // کارکرد
    BodyCondition,  // وضعیت بدنه
    ChassisCondition,// وضعیت شاسی‌ها
    Mechanical,     // وضعیت موتور و فنی
    Gearbox,        // نوع گیربکس (متنی)
    Photos,         // آپلود عکس (چندتایی)
    Price,          // فقط برای ادمین
    Extra,          // فقط برای ادمین
    Done
}
