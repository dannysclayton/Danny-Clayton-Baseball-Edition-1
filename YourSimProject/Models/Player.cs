using System.Collections.Generic;
using System.Linq;
using System;

namespace YourSimProject.Models
{
public class Player
{
    // --- Season Performance Bonuses ---
    public double PitcherPerformanceBonus { get; set; } // Accumulates scoreless innings, strikeouts, etc.
    public double HitterPerformanceBonus { get; set; } // Accumulates hits, home runs, etc.

    // Resets all season bonuses (call at season end)
    public void ResetSeasonBonuses()
    {
        PitcherPerformanceBonus = 0;
        HitterPerformanceBonus = 0;
        StrikeoutBonus = 0;
        DoubleBonus = 0;
        TripleBonus = 0;
        HomeRunBonus = 0;
    }
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
    // Spectacular play chance accumulator (season)
    public double SpectacularPlayAccumulator { get; set; }
    // Clutch label for pitchers (season flag)
    public bool IsClutch { get; set; }
    // Season bonuses (optional use)
    public int SeasonPitchCapacityBonus { get; set; }
    public int SeasonRatingBonus { get; set; }
}
}
