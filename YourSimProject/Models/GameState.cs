public class GameState
{
    public required string CurrentBatterName { get; set; }
    public required string CurrentPitcherName { get; set; }
    public int CurrentInning { get; set; }
    public int Outs { get; set; }
    public int RunnersOnBases { get; set; } // Uses bitwise flags (e.g., 1=1st, 2=2nd, 4=3rd)

    // Other critical mid-game variables
    public required string BattingTeamName { get; set; }
    public required string PitchingTeamName { get; set; }
    public int ScoreHome { get; set; }
    public int ScoreAway { get; set; }
}