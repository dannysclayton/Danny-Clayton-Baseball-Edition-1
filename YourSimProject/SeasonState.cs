// Removed invalid using statement
using System;
using System.Collections.Generic;
using System.Linq; // For methods like .ToList()

public class SeasonState
{
    // 1. SEASON SETUP SETTINGS (from the Season Creation Flow)

    /// <summary>
    /// Stores the results of the "Pick Season Length" step.
    /// Example: { "LengthOfInterRegionSeries", "3 Games" }
    /// </summary>
    public Dictionary<string, string> SetupSettings { get; set; } = new();

    /// <summary>
    /// Defines the type of control for the season.
    /// Example: "One Team User Control"
    /// </summary>
    public string ControlType { get; set; }

    /// <summary>
    /// Defines the playoff structure chosen.
    /// Example: "College - Mixed Format"
    /// </summary>
    public string PlayoffFormat { get; set; }

    /// <summary>
    /// The name assigned when the season was saved.
    /// </summary>
    public string SeasonName { get; set; }
    
    // 2. LEAGUE DATA (The entire active league structure)

    /// <summary>
    /// The list of all active Conferences, Regions, and Teams participating in the season.
    /// This holds the current W/L records for standings.
    /// </summary>
    public List<Conference> ActiveConferences { get; set; } = new();

    /// <summary>
    /// A consolidated list of all Teams for quick lookups.
    /// NOTE: Team objects must be deep copies or references from ActiveConferences 
    /// to ensure stats and W/L records are consistent.
    /// </summary>
    public List<Team> AllActiveTeams { get; set; } = new();

    // 3. CURRENT GAME/SCHEDULE STATE (For mid-game saves or schedule tracking)
    
    /// <summary>
    /// The schedule index or date of the last completed game.
    /// Used to determine the next game to play.
    /// </summary>
    public int LastCompletedGameIndex { get; set; }

    /// <summary>
    /// The full schedule of all games in the season.
    /// </summary>
    public List<Game> FullSchedule { get; set; } = new();

    /// <summary>
    /// True if the game was saved mid-play (e.g., during an At-Bat).
    /// </summary>
    public bool IsMidGameSave { get; set; }

    /// <summary>
    /// Serialized data of the current game state (score, runners, pitcher, batter) 
    /// if IsMidGameSave is true.
    /// </summary>
    public GameState CurrentGameState { get; set; } = new GameState {
        CurrentBatterName = "",
        CurrentPitcherName = "",
        BattingTeamName = "",
        PitchingTeamName = "",
        CurrentInning = 1,
        Outs = 0,
        RunnersOnBases = 0,
        ScoreHome = 0,
        ScoreAway = 0
    };
    
    // 4. HISTORICAL/CAREER STAT POINTERS
    
    /// <summary>
    /// Pointers (file paths or IDs) to the cumulative career stats for all players in this league.
    /// This ensures player stats carry over year-to-year.
    /// </summary>
    public Dictionary<string, string> CareerStatPointers { get; set; } = new(); 
}