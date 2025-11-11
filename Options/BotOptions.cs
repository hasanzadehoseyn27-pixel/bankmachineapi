namespace BankeKhodroBot.Options;

public class BotOptions
{
    public string Token { get; set; } = string.Empty;

    // contact shown in group:
    public string ContactPhone { get; set; } = "09127475355";
    public string ContactName { get; set; } = "کیوان راشدی";

    // where to host WebApp (e.g., https://your-domain.com/webapp/)
    public string WebAppUrl { get; set; } = "http://localhost:5000/webapp/";
}
