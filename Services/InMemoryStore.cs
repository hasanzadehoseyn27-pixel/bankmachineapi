using System.Collections.Concurrent;
using BankeKhodroBot.Models;

namespace BankeKhodroBot.Services;

public interface ISessionStore
{
    ConcurrentDictionary<long, Session> Sessions { get; }
}

public interface IPendingAdStore
{
    ConcurrentDictionary<string, CarAd> PendingAds { get; }
}

public class MemoryStore : ISessionStore, IPendingAdStore
{
    public ConcurrentDictionary<long, Session> Sessions { get; } = new();
    public ConcurrentDictionary<string, CarAd> PendingAds { get; } = new();
}
