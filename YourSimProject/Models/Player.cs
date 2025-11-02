using System.Collections.Generic;
using System.Linq;
using System;

public class Player
{
    // --- Core Identity (MUST have public set for JSON persistence) ---
    public string TeamName { get; set; } = "Unassigned"; // The team the player is currently assigned to (Fixes persistence)
    public string Name { get; set; } = "New Player";
    public string ClassLevel { get; set; } = "Freshman"; // Freshman, Sophomore, Junior, Senior
    
    // Positions must be initialized to ensure JSON can deserialize the list
    public List<string> Positions { get; set; } = new List<string>(); 

    // --- Ratings ---
    public int BattingRating { get; set; } // Base d6 roll (1-6)
    public int PitchingRating { get; set; } // Base d6 roll (1-6)
    
    // --- Calculated Property (Must NOT have a setter) ---
    public int ClassModifier
    {
        get
        {
            return ClassLevel switch
            {
                "Freshman" => 0,
                "Sophomore" => 1,
                "Junior" => 2,
                "Senior" => 3,
                _ => 0,
            };
        }
    }

    // --- Season Management (MUST have public set for JSON persistence) ---
    public int EligibleSeasons { get; set; }
    public bool IsRedshirted { get; set; }
    public Dictionary<string, int> PositionAtBats { get; set; } = new Dictionary<string, int>();

    // --- Accumulating Modifiers (MUST have public set for JSON persistence) ---
    public double ErrorAccumulator { get; set; }
    public double BalkWildPitchAccumulator { get; set; }
    public double PickoffAccumulator { get; set; }
    public double StrikeoutBonus { get; set; }
    public double DoubleBonus { get; set; }
    public double TripleBonus { get; set; }
    public double HomeRunBonus { get; set; }
}
