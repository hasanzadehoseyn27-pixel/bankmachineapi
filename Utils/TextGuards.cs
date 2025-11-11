using System.Text.RegularExpressions;

namespace BankeKhodroBot.Utils;

public static class TextGuards
{
    static readonly Regex RxLink = new(@"(https?:\/\/|www\.|t\.me\/|telegram\.me\/)\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static readonly Regex RxUser = new(@"@\w{5,}", RegexOptions.Compiled);
    static readonly Regex RxPhone = new(@"(?:(?:\+|00)\d{1,3}[\s\-]?)?(?:\(?\d{2,4}\)?[\s\-]?){2,4}\d{2,4}", RegexOptions.Compiled);

    public static bool ContainsContactOrLink(string? t)
        => !string.IsNullOrWhiteSpace(t) && (RxLink.IsMatch(t!) || RxUser.IsMatch(t!) || RxPhone.IsMatch(t!));
}
