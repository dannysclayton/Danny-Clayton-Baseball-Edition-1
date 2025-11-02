// Models/PlayerPitchingStats.cs
public class PlayerPitchingStats
{
    public required string PlayerName { get; set; }
    public int G, GS, W, L, SV, HLD, CG, SHO, QS, BF, H, R, ER, BB, SO, HR, WP, HB, BK, PK, DPInduced, CSAgainst;
    public double ERA, WHIP, IP;
    public int PitchCountCapacity { get; set; } // Initial Pitch Count [cite: 210]
    public int CurrentPitchCount { get; set; }
    public bool IsFatigued { get; set; }
}