// Models/PlayerBattingStats.cs
public class PlayerBattingStats
{
    public required string PlayerName { get; set; }
    public int G, PA, AB, H, R, RBI, _2B, _3B, HR, SB, CS, BB, SO, HBP, SF, SH, GDP;
    public double AVG, OBP, SLG, SBPercent;
    public int BuntAttempts, BuntSuccesses;
}