
public class Team
{
    public required string Name { get; set; }
    public required string RegionName { get; set; }
    public required string ConferenceName { get; set; }

    public required Coach Coach { get; set; } // Assigned Coach [cite: 385]
    public List<Player> Roster { get; set; } = new(); // All 30 players [cite: 54, 64]
    public List<Player> Redshirts { get; set; } = new(); // Up to 5 redshirted players [cite: 54, 66]

    // Roster Assignments
    public Dictionary<string, Player> FieldingLineup { get; set; } = new(); // C, 1B, 2B, SS, 3B, LF, CF, RF, DH [cite: 56, 81]
    // FieldingAssignments now supports both starter and backup per position
    public Dictionary<string, (Player Starter, Player? Backup)> FieldingAssignments { get; set; } = new(); // Manual fielding assignments with backup
    public List<Player> BattingLineup { get; set; } = new(); // 9 players in order [cite: 57]
    public List<Player> PitchingRotation { get; set; } = new(); // Starters and relievers [cite: 56]
    
    // In-Season Stats
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int RegionWins { get; set; }
    public int RegionLosses { get; set; }
    public int RunsScored { get; set; } // [cite: 1090]
    public int RunsAllowed { get; set; } // [cite: 1090]
    public double GamesBehind { get; set; } // [cite: 1090]
    
    // Aesthetic Data (Paths to files)
    public required string LogoPath { get; set; }
    public required string UniformPath { get; set; }
    public required string TeamPhotoPath { get; set; }
    public required string TeamFieldingPhotoPath { get; set; }
    public required string TeamBattingPhotoPath { get; set; }
    public required string TeamPitchingPhotoPath { get; set; }
}