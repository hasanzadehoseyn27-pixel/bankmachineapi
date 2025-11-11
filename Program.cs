using BankeKhodroBot.Handlers;
using BankeKhodroBot.Options;
using BankeKhodroBot.Services;
using BankeKhodroBot.TelegramApi;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// ===== Configs =====
builder.Configuration
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));

// ===== Telegram HTTP sender + Telegram.Bot client =====
string GetToken(IServiceProvider sp)
{
    var token = sp.GetRequiredService<IOptions<BotOptions>>().Value.Token;
    if (string.IsNullOrWhiteSpace(token))
        token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
              ?? Environment.GetEnvironmentVariable("Bot__Token");
    if (string.IsNullOrWhiteSpace(token))
        throw new InvalidOperationException("Bot token is missing.");
    return token;
}

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var token = GetToken(sp);
    return new TelegramBotClient(token);
});

builder.Services.AddHttpClient<ITgSender, TgHttp>((sp, http) =>
{
    var token = GetToken(sp);
    http.BaseAddress = new Uri($"https://api.telegram.org/bot{token}/");
});

// ===== RuntimeConfig (اشکال شما دقیقا از این خط نبودن بود) =====
builder.Services.AddSingleton<IRuntimeConfig, RuntimeConfig>();

// ===== Stores =====
builder.Services.AddSingleton<MemoryStore>();
builder.Services.AddSingleton<ISessionStore>(sp => sp.GetRequiredService<MemoryStore>());
builder.Services.AddSingleton<IPendingAdStore>(sp => sp.GetRequiredService<MemoryStore>());

// ===== Handlers =====
builder.Services.AddSingleton<PrivateFormHandler>();
builder.Services.AddSingleton<GroupModerationHandler>();
builder.Services.AddSingleton<AdminCallbackHandler>();
builder.Services.AddSingleton<IUpdateHandler, UpdateDispatcher>();

// ===== Hosted services =====
builder.Services.AddHostedService<StartupService>();
builder.Services.AddHostedService<PollingService>();

builder.Logging.AddConsole();

var app = builder.Build();

// ===== Static WebApp (wwwroot/webapp/index.html) =====
app.UseDefaultFiles();   // index.html را اتومات سرو می‌کند
app.UseStaticFiles();    // wwwroot

// health
app.MapGet("/healthz", () => Results.Ok("ok"));

// APIهای WebApp (اگر AdsEndpoints.cs دارید)
AdsEndpoints.Map(app);

app.Run();
