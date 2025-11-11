namespace BankeKhodroBot.Models;

public class Session
{
    public Step Step { get; set; } = Step.None;
    public CarAd Ad { get; set; } = new();
}
