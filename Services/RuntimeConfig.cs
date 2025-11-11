using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BankeKhodroBot.Services;

public interface IRuntimeConfig
{
    long AdminChatId { get; }
    long GroupChatId { get; }

    void SetAdmin(long id);
    void SetGroup(long id);
}

public class RuntimeConfig : IRuntimeConfig
{
    private readonly ILogger<RuntimeConfig> _log;
    private readonly string _path;
    private readonly object _lockObj = new();

    private RuntimeState _state;

    // مدل داخلی برای ذخیره روی دیسک
    private class RuntimeState
    {
        public long AdminChatId { get; set; } = 0;
        public long GroupChatId { get; set; } = 0;
    }

    public RuntimeConfig(ILogger<RuntimeConfig> log)
    {
        _log = log;
        _path = Path.Combine(AppContext.BaseDirectory, "global.json"); // فایل ذخیره
        _state = Load();
    }

    public long AdminChatId { get { lock (_lockObj) return _state.AdminChatId; } }
    public long GroupChatId { get { lock (_lockObj) return _state.GroupChatId; } }

    public void SetAdmin(long id)
    {
        lock (_lockObj)
        {
            _state.AdminChatId = id;
            Save();
        }
    }

    public void SetGroup(long id)
    {
        lock (_lockObj)
        {
            _state.GroupChatId = id;
            Save();
        }
    }

    private RuntimeState Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var s = JsonSerializer.Deserialize<RuntimeState>(json);
                if (s != null) return s;
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Load runtime config failed.");
        }
        return new RuntimeState();
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(
                _state,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(_path, json);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Save runtime config failed.");
        }
    }
}
