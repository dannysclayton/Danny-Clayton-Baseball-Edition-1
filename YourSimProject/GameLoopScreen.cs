using System;
using System.Threading;
using System.Linq;
using YourSimProject.Models;
using YourSimProject.Services;

namespace YourSimProject
{
public class GameLoopScreen
{
    // --- Random ---
    private readonly Random _rnd = new Random();

    // --- Pitcher Fatigue System ---
    private class PitcherState
    {
        public int MaxPitchCount;        // rating * 10 + (ClassModifier * 0 assumed)
        public int CurrentPitches;       // accumulates per outcome
        public int StrikeoutsThisInning; // for bonuses
        public int RunsThisInning;       // for scoreless inning bonus
        public int RatingPenalty;        // computed from fatigue (1 per +10 over max)
    }
    private readonly System.Collections.Generic.Dictionary<string, PitcherState> _pitcherStates = new System.Collections.Generic.Dictionary<string, PitcherState>();
    private readonly GameEngine _engine;
    private readonly System.Collections.Generic.List<string> playByPlayLog = new System.Collections.Generic.List<string>();

    
    // --- Current Game State (Simplified for placeholder) ---
    private int inning = 1;
    private int outs = 0;
    private int scoreHome = 0;
    private int scoreAway = 0;
    private string currentTeamBatting = "";
    private string homeTeamName = "";
    private string awayTeamName = "";
    private string location = "";
    private string controlMode = ""; // NEW: Store control type

    // --- Bases ---
    private class Bases
    {
        public Player? First;
        public Player? Second;
        public Player? Third;
        public bool AnyRunnerOnBase => First != null || Second != null || Third != null;
        public bool RunnerOnThird => Third != null;
    }
    private Bases bases = new Bases();

    // --- Charts ---
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartOffBasic = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"3B"},{12,"GO"},{13,"K"},{14,"BB"},{15,"1B"},{16,"FO"},
        {21,"FO"},{22,"2B"},{23,"GO"},{24,"LO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"2B"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"LO"},{42,"K"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"GO"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"PO"},{62,"GO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartOffAverage = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"3B"},{12,"GO"},{13,"K"},{14,"BB"},{15,"1B"},{16,"FO"},
        {21,"FO"},{22,"2B"},{23,"GO"},{24,"GO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"2B"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"GO"},{42,"2B"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"2B"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"3B"},{62,"GO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartOffSenior = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"HR"},{12,"GO"},{13,"K"},{14,"BB"},{15,"1B"},{16,"FO"},
        {21,"LO"},{22,"2B"},{23,"GO"},{24,"GO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"2B"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"LO"},{42,"2B"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"2B"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"3B"},{62,"GO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartPitBasic = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"3B"},{12,"LO"},{13,"K"},{14,"BB"},{15,"1B"},{16,"FO"},
        {21,"FO"},{22,"2B"},{23,"GO"},{24,"GO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"2B"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"LO"},{42,"K"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"GO"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"PO"},{62,"GO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartPitJourneyman = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"3B"},{12,"GO"},{13,"K"},{14,"BB"},{15,"GO"},{16,"FO"},
        {21,"FO"},{22,"1B"},{23,"GO"},{24,"LO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"1B"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"LO"},{42,"2B"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"2B"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"3B"},{62,"GO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };
    private static readonly System.Collections.Generic.Dictionary<int, string> ChartPitAce = new System.Collections.Generic.Dictionary<int, string>
    {
        {11,"2B"},{12,"GO"},{13,"K"},{14,"BB"},{15,"GO"},{16,"FO"},
        {21,"FO"},{22,"1B"},{23,"GO"},{24,"GO"},{25,"1B"},{26,"K"},
        {31,"FO"},{32,"FO"},{33,"GO"},{34,"PO"},{35,"1B"},{36,"BB"},
        {41,"LO"},{42,"1B"},{43,"GO"},{44,"1B"},{45,"BB"},{46,"K"},
        {51,"FO"},{52,"GO"},{53,"2B"},{54,"PO"},{55,"1B"},{56,"K"},
        {61,"3B"},{62,"LO"},{63,"FO"},{64,"K"},{65,"LO"},{66,"HR"}
    };

    public GameLoopScreen(GameEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Initializes and starts the exhibition game loop.
    /// </summary>
    public void StartExhibitionGame(string teamA, string teamB, string gameLocation, string controlType)
    {
        // Diagnostic: Show loaded teams and lineups
        Console.WriteLine("[DIAGNOSTIC] Exhibition Game Setup");
        Console.WriteLine($"Team A: {teamA}");
        var teamAObj = _engine.GetTeamByName(teamA);
        if (teamAObj != null)
        {
            Console.WriteLine($"  Batting Lineup: {string.Join(", ", teamAObj.BattingLineup.Select(p => p.Name))}");
            Console.WriteLine($"  Pitching Rotation: {string.Join(", ", teamAObj.PitchingRotation.Select(p => p.Name))}");
        }
        else
        {
            Console.WriteLine("  [ERROR] Team A not found!");
        }
        Console.WriteLine($"Team B: {teamB}");
        var teamBObj = _engine.GetTeamByName(teamB);
        if (teamBObj != null)
        {
            Console.WriteLine($"  Batting Lineup: {string.Join(", ", teamBObj.BattingLineup.Select(p => p.Name))}");
            Console.WriteLine($"  Pitching Rotation: {string.Join(", ", teamBObj.PitchingRotation.Select(p => p.Name))}");
        }
        else
        {
            Console.WriteLine("  [ERROR] Team B not found!");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);

        // Reset state for new game
        inning = 1;
        outs = 0;
        scoreHome = 0;
        scoreAway = 0;
        
        // Set initial state from Exhibition setup
        awayTeamName = teamA; // Team A starts batting (Away Team)
        homeTeamName = teamB; // Team B is fielding (Home Team)
        currentTeamBatting = awayTeamName;
        location = gameLocation;
        controlMode = controlType; // Store the control type
        
        RunGameLoop();
    }

    private void RunGameLoop()
    {
        // This is the heart of the simulation—it runs until the game ends (9 innings or mercy rule).
        while (inning <= 9)
        {
            DisplayGameStatus();
            DisplayPlayByPlayLog();

            // Get current batter and pitcher from lineups (placeholder logic)
            Team? battingTeam = currentTeamBatting == awayTeamName ? _engine.GetTeamByName(awayTeamName) : _engine.GetTeamByName(homeTeamName);
            Team? pitchingTeam = currentTeamBatting == awayTeamName ? _engine.GetTeamByName(homeTeamName) : _engine.GetTeamByName(awayTeamName);

            if (battingTeam == null || pitchingTeam == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: One or both teams could not be found. Please check team names and try again.");
                Console.ResetColor();
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey(true);
                SaveAndQuit();
                return;
            }

            Player? batter = battingTeam.BattingLineup.Count > 0 ? battingTeam.BattingLineup[0] : null; // TODO: Use actual batting order index
            Player? pitcher = pitchingTeam.PitchingRotation.Count > 0 ? pitchingTeam.PitchingRotation[0] : null; // TODO: Use actual rotation index

            if (batter == null || pitcher == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Batting lineup or pitching rotation is empty. Please set lineups before starting the game.");
                Console.ResetColor();
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey(true);
                SaveAndQuit();
                return;
            }

            bool turnComplete = false;
            while (!turnComplete)
            {
                if (controlMode == "User vs User" || (controlMode != "Computer vs Computer" && currentTeamBatting == awayTeamName))
                {
                    Console.WriteLine($"\n[USER OFFENSE] Choose your action:");
                    Console.WriteLine(" [1] Swing | [2] Bunt | [3] Steal | [4] Substitution | [5] View Box Score | [Q] Quit/Save Game");
                    Console.Write("Enter choice: ");
                    string input = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();
                    if (input == "V" || input == "5")
                    {
                        ViewBoxScore();
                        continue;
                    }
                    else if (input == "Q")
                    {
                        SaveAndQuit();
                        return;
                    }
                    else if (input == "1" || input == "2" || input == "3" || input == "4")
                    {
                        SimulateAtBat(batter, pitcher);
                        turnComplete = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                    }
                }
                else
                {
                    SimulateAtBat(batter, pitcher);
                    turnComplete = true;
                }
            }

            if (CheckMercyRule()) break;
        }

        // Game Over screen placeholder
        EndGameScreen();
    }
    
    private void SimulateAtBat(Player currentBatter, Player currentPitcher)
    {
        // Gather at-bat details
        string batterName = currentBatter?.Name ?? "Unknown";
        string pitcherName = currentPitcher?.Name ?? "Unknown";
        // --- Suicide Squeeze Early Resolution ---
        // Offensive Coach uses Hit and Run with runner on 3B to attempt a suicide squeeze BEFORE normal at-bat resolution.
        // We prompt for options first, then detect squeeze conditions and resolve.
    int batterRating = GetEffectiveBatterRating(currentBatter!);
    int pitcherRating = GetEffectivePitcherRating(currentPitcher!);

    // Offensive/Defensive options
    Console.WriteLine("\nChoose Offensive Option:");
    Console.WriteLine("1. Normal At Bat\n2. Power\n3. Bunt\n4. Sacrifice\n5. Steal Base\n6. Double Steal\n7. Hit and Run\n8. Safe\n9. Substitution");
    int batterChoice = GetUserChoice(1,9);
    string batterOption = GetBatterOption(batterChoice);

    Console.WriteLine("\nChoose Defensive Option:");
    Console.WriteLine("1. Normal Pitch\n2. Pitch Out\n3. Infield In\n4. Double Play Depth\n5. Outfield In\n6. No Doubles\n7. Intentional Walk\n8. Substitution");
    int pitcherChoice = GetUserChoice(1,8);
    string pitcherOption = GetPitcherOption(pitcherChoice);

        // Suicide Squeeze attempt logic (Hit and Run with runner on 3B and <2 outs)
        if (batterOption == "Hit and Run" && bases.Third != null && outs < 2)
        {
            bool resolved = ResolveSuicideSqueeze(currentBatter!, currentPitcher!, pitcherOption);
            if (resolved) return; // play concluded
        }

    // Intentional Walk logic (instant award of first base)
    if (pitcherOption == "Intentional Walk")
    {
        playByPlayLog.Add($"[IW] Defense issues intentional walk to {batterName}");
        AdvanceOnWalk(currentBatter!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.Walk); // treat as walk for fatigue accounting
        return;
    }

    // Handle pre-play options like steals, subs, etc.
    if (batterOption == "Steal Base") { ResolveSteal(pitcherOption); return; }
    if (batterOption == "Double Steal") { ResolveDoubleSteal(pitcherOption); return; }
    if (batterOption == "Substitution") { playByPlayLog.Add("[OFFENSE] Substitution menu (stub)"); return; }
    if (pitcherOption == "Substitution") { playByPlayLog.Add("[DEFENSE] Substitution menu (stub)"); }

    // Pitch Out passive effects if NOT a steal attempt
    if (pitcherOption == "Pitch Out" && batterOption != "Steal Base" && batterOption != "Double Steal")
    {
        double r = _rnd.NextDouble();
        if (r < 0.50)
        {
            playByPlayLog.Add("[PITCH OUT] Free pass granted (walk chance triggered)." );
            AdvanceOnWalk(currentBatter!);
            AddPitchCount(currentPitcher!, OutcomePitchCost.Walk);
            return;
        }
        else if (r < 0.75)
        {
            // Treat as a soft single
            playByPlayLog.Add("[PITCH OUT] Batter sneaks a hit on pitch out!");
            AdvanceOnHit("1B", currentBatter!, pitcherOption);
            AddPitchCount(currentPitcher!, OutcomePitchCost.Hit);
            return;
        }
    }

    // Balk / Wild Pitch / Pickoff pre-play checks (if runners on base)
    if (bases.AnyRunnerOnBase)
    {
    AttemptPickoff(currentPitcher!);
    AttemptBalkOrWildPitch(currentPitcher!);
    }

    // Roll-Off (Updated): d6 + rating (reroll ties)
    int batterRoll, pitcherRoll;
    do {
        batterRoll = _rnd.Next(1,7) + batterRating;
        pitcherRoll = _rnd.Next(1,7) + pitcherRating;
    } while (batterRoll == pitcherRoll);

    bool batterWins = batterRoll > pitcherRoll;
    int chartRoll1 = _rnd.Next(1,7);
    int chartRoll2 = _rnd.Next(1,7);
    int chartNumber = chartRoll1 * 10 + chartRoll2;

    // Chart Selection Logic (by Rating + ClassModifier)
    string chartType = "OffBasic";
    string outcome;
    if (batterWins)
    {
    int totalRating = currentBatter!.BattingRating + currentBatter.ClassModifier;
        chartType = totalRating >= 9 ? "OffSenior" : (totalRating >= 7 ? "OffAverage" : "OffBasic");
        outcome = LookupChart(chartType, chartNumber);
    }
    else
    {
    int basePitch = GetBasePitchingRating(currentPitcher!);
    int totalRating = basePitch + currentPitcher!.ClassModifier - GetFatiguePenalty(currentPitcher!);
        chartType = totalRating >= 9 ? "PitAce" : (totalRating >= 7 ? "PitJourneyman" : "PitBasic");
        outcome = LookupChart(chartType, chartNumber);
    }

    string result = ApplyOptionAdjustments(outcome, batterOption, pitcherOption);
    // Apply hitter season bonuses to bias hits toward extra-base outcomes
    result = ApplyHitterBonuses(currentBatter!, result);
    // Apply pitcher strikeout bonus to bias outs toward K
    result = ApplyPitcherStrikeoutBias(currentPitcher!, result);

    // Introduce Hit-By-Pitch chance (independent layer) unless intentional walk
    result = MaybeApplyHBP(result, currentPitcher!, currentBatter!, batterOption, pitcherOption);

    // Sacrifice Fly advanced resolution tables (runners tagging outcomes) if we flagged FO_SF
    if (result == "FO_SF")
    {
        ResolveSacrificeFly(currentBatter!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
        if (outs == 3) EndHalfInning();
        return;
    }

    // Detailed play-by-play log
    string playLog = $"[AT-BAT] {batterName} vs {pitcherName} | Offense: {batterOption} | Defense: {pitcherOption} | BatRoll: {batterRoll} | PitchRoll: {pitcherRoll} | Chart: {chartType} | Roll: {chartNumber} | Result: {result}";
    playByPlayLog.Add(playLog);

    // Update game state and add result lines
    // Fielding error / passed ball evaluation before applying outs or hits
    result = ApplyFieldingErrorLayer(result, currentPitcher!, currentBatter!);

    // Handle explicit Sac Fly before generic branches
        if (result == "FO_SF")
        {
            playByPlayLog.Add("[RESULT] Sacrifice Fly!");
            ScoreAndClear(ref bases.Third);
            outs++;
            AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
        }
    else if (result == "HR_ROB")
    {
        // Spectacular robbed home run turned into an out
        playByPlayLog.Add("[ROBBED] Home run bid taken away at the wall! Batter is out.");
        outs++;
        AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
    }
    else if (result == "HR")
    {
        // Detailed home run resolver with solo and multi-run narratives
        ResolveHomeRun(currentBatter!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.Hit);
        if (currentBatter != null)
        {
            currentBatter.HitterPerformanceBonus += 0.5; // Extra HR performance bonus
            currentBatter.HomeRunBonus += 0.01; // +1% future HR chance
        }
    }
    else if (result == "K")
    {
        ResolveStrikeoutWithRunners(currentBatter!, currentPitcher!, batterOption, pitcherOption);
    }
    else if (result == "K_PB")
    {
        // Dropped third strike already advanced runners/batter in error layer
        playByPlayLog.Add("[RESULT] Strikeout on dropped third; batter reaches.");
        AddPitcherStrikeoutBonus(currentPitcher!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.Strikeout);
    }
    else if (result == "BB")
    {
        ResolveWalk(currentBatter!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.Walk);
    }
    else if (result == "HBP")
    {
        ResolveHitByPitch(currentBatter!);
        AddPitchCount(currentPitcher!, OutcomePitchCost.Walk);
    }
    else if (result == "GO" || result == "GO_DP" || result == "FO" || result == "PU" || result == "PO" || result == "LO")
    {
        // Handle special spectacular DP marker first
        if (result == "GO_DP")
        {
            playByPlayLog.Add("[RESULT] Spectacular double play turned on the grounder!");
            // Simple DP: batter and runner on first out
            int addOuts = Math.Min(2, 3 - outs);
            outs += addOuts;
            if (bases.First != null) bases.First = null;
            AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
        }
        else if (result == "GO")
        {
            ResolveGroundoutWithRunners(batterOption, pitcherOption);
            AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
        }
        else
        {
            // Flyouts / popups / liners
            if (result == "LO")
            {
                ResolveLineOutWithRunners(batterOption, pitcherOption);
            }
            else if (result == "PO" || result == "PU")
            {
                ResolvePopOutWithRunners(batterOption, pitcherOption, result);
            }
            else if (result.EndsWith("_ROB"))
            {
                // Any robbed hit becomes an out (already narrated in fielding layer)
                outs++;
            }
            else
            {
                ResolveFlyOutWithRunners(batterOption, pitcherOption, result);
            }
            AddPitchCount(currentPitcher!, OutcomePitchCost.BattedOut);
        }
        if (outs == 3)
        {
            // Scoreless inning bonus
            if (GetCurrentInningRunsAgainst(currentPitcher!) == 0) AddPitcherScorelessBonus(currentPitcher!);
        }
    }
    else
    {
        playByPlayLog.Add($"[RESULT] Hit! ({result})");
        // Singles with runners use detailed advancement charts
        if (result == "1B")
        {
            if (!bases.AnyRunnerOnBase)
            {
                playByPlayLog.Add($"[SINGLE NARRATIVE] {NarrativeHelpers.GetSoloSingleNarrative()}");
                // Place batter on first
                if (bases.First == null) bases.First = currentBatter; else { bases.Second = bases.First; bases.First = currentBatter; }
            }
            else
            {
                ResolveSingleWithRunners(currentBatter!, batterOption, pitcherOption);
            }
        }
        else if (result == "2B")
        {
            // Double narratives (currently bases empty or runner on first fully specified; partial list for runner on first)
            ResolveDoubleWithRunners(currentBatter!, batterOption, pitcherOption);
        }
        else if (result == "3B")
        {
            // Triple narratives and advancement
            ResolveTripleWithRunners(currentBatter!, batterOption, pitcherOption);
        }
        else
        {
            AdvanceOnHit(result, currentBatter!, pitcherOption);
        }
    AddPitchCount(currentPitcher!, OutcomePitchCost.Hit);
        if (currentBatter != null)
        {
            currentBatter.HitterPerformanceBonus += 0.1; // legacy bonus
            if (result.StartsWith("2B")) currentBatter.DoubleBonus += 0.0025; // +0.25%
            else if (result.StartsWith("3B")) currentBatter.TripleBonus += 0.005; // +0.50%
            else if (result.StartsWith("HR")) currentBatter.HomeRunBonus += 0.01; // +1%
        }
    }

    if (outs == 3)
    {
        EndHalfInning();
    }

    }

    // --- Option Helper Methods ---
    private int GetUserChoice(int min, int max)
    {
        int choice;
        do {
            Console.Write("Enter choice: ");
            var input = Console.ReadLine();
            int.TryParse(input, out choice);
        } while (choice < min || choice > max);
        return choice;
    }
    private string GetBatterOption(int choice)
    {
        string[] options = {"Normal", "Power", "Bunt", "Sacrifice", "Steal Base", "Double Steal", "Hit and Run", "Safe", "Substitution"};
        return options[choice-1];
    }
    private string GetPitcherOption(int choice)
    {
        string[] options = {"Normal", "Pitch Out", "Infield In", "Double Play Depth", "Outfield In", "No Doubles", "Intentional Walk", "Substitution"};
        return options[choice-1];
    }
    // --- Option adjustment helpers ---
    private string ApplyOptionAdjustments(string baseResult, string batterOption, string pitcherOption)
    {
        string res = baseResult;

        // Power: +25% K; if runner on 3rd, +25% FO (sac fly). Hit upgrades unless No Doubles
        if (batterOption == "Power")
        {
            double roll = _rnd.NextDouble();
            if (roll < 0.25) res = "K";
            else if (bases.RunnerOnThird && roll < 0.50) res = "FO_SF"; // mark sac fly
            else if (IsHit(res) && pitcherOption != "No Doubles") res = UpgradeHit(res);
            else if (res == "GO" && bases.AnyRunnerOnBase && pitcherOption != "No Doubles") res = "GO_DP"; // double play marker
        }

        // Bunt: success chance affected by infield depth and ratings
        if (batterOption == "Bunt")
        {
            double success = 0.50; // base
            if (pitcherOption == "Infield In") success -= 0.50; // -50%
            if (pitcherOption == "Double Play Depth") success += 0.25; // +25%
            // rating compare: batter vs pitcher & catcher (approx: use batting vs pitching)
            if (GetEffectiveBatterRatingSafe() > GetEffectivePitcherRatingSafe()) success += 0.50; else success -= 0.25;
            if (_rnd.NextDouble() < success)
            {
                res = "SAC"; // sacrifice bunt
            }
            else
            {
                res = _rnd.NextDouble() < 0.5 ? "PO" : "FO";
            }
        }

        // Sacrifice: improve advance chances
        if (batterOption == "Sacrifice")
        {
            res = "SAC";
        }

        // Hit and Run: increase K chance; enable extra advance on hits
        if (batterOption == "Hit and Run")
        {
            if (_rnd.NextDouble() < 0.25) res = "K";
        }

        // Defensive options partial effects
        if (pitcherOption == "Pitch Out")
        {
            // no direct effect unless steal (handled elsewhere)
        }
        else if (pitcherOption == "Outfield In" && res == "2B")
        {
            // Outfield In: a double will result in an additional base and +25% chance runner scores (handled in advance logic)
        }
        else if (pitcherOption == "No Doubles" && res == "2B")
        {
            if (_rnd.NextDouble() < 0.5) res = "1B"; // downgrade 50%
        }

        return res;
    }

    // --- Ratings and Chart Lookup ---
    private int GetEffectiveBatterRating(Player batter) => Math.Clamp(batter.BattingRating + batter.ClassModifier, 1, 12);
    private int GetEffectiveBatterRatingSafe() => 6;
    private int GetEffectivePitcherRatingSafe() => 6;
    private int GetBasePitchingRating(Player pitcher)
    {
        // If not a pitcher by position, default rating is 4 per hidden rule
        bool isPitcher = pitcher.Positions.Any(p => p.Equals("P", StringComparison.OrdinalIgnoreCase));
        return isPitcher ? pitcher.PitchingRating : 4;
    }
    private int GetEffectivePitcherRating(Player pitcher)
    {
        int baseRating = GetBasePitchingRating(pitcher) + pitcher.ClassModifier;
        int penalty = GetFatiguePenalty(pitcher);
        return Math.Max(1, baseRating - penalty);
    }
    private int GetFatiguePenalty(Player pitcher)
    {
        var st = GetPitcherState(pitcher);
        int over = Math.Max(0, st.CurrentPitches - st.MaxPitchCount);
        return over / 10; // -1 per 10 over
    }
    private string LookupChart(string chartType, int roll)
    {
        return chartType switch
        {
            "OffSenior" => ChartOffSenior.GetValueOrDefault(roll, "GO"),
            "OffAverage" => ChartOffAverage.GetValueOrDefault(roll, "GO"),
            "OffBasic" => ChartOffBasic.GetValueOrDefault(roll, "GO"),
            "PitAce" => ChartPitAce.GetValueOrDefault(roll, "GO"),
            "PitJourneyman" => ChartPitJourneyman.GetValueOrDefault(roll, "GO"),
            _ => ChartPitBasic.GetValueOrDefault(roll, "GO"),
        };
    }

    private bool IsHit(string res) => res == "1B" || res == "2B" || res == "3B" || res == "HR";
    private string UpgradeHit(string res) => res switch
    {
        "1B" => "2B",
        "2B" => "3B",
        "3B" => "HR",
        _ => res
    };

    // --- Season Accumulating Bonus Application ---
    private string ApplyHitterBonuses(Player batter, string result)
    {
        if (!IsHit(result)) return result;
        double roll = _rnd.NextDouble();
        // Home run upgrade chance (from existing hit) based on home run bonus
        if (result == "3B" && roll < batter.HomeRunBonus) return "HR";
        if (result == "2B")
        {
            // Triple chance from triple bonus
            if (roll < batter.TripleBonus) return "3B";
            // Additional upgrade to HR if both triple/home run bonuses are significant
            if (roll < (batter.TripleBonus * 0.25 + batter.HomeRunBonus * 0.25)) return "HR";
        }
        if (result == "1B")
        {
            if (roll < batter.DoubleBonus) return "2B";
            if (roll < (batter.DoubleBonus * 0.25 + batter.TripleBonus * 0.10)) return "3B";
        }
        return result;
    }
    private string ApplyPitcherStrikeoutBias(Player pitcher, string result)
    {
        if (result != "K" && _rnd.NextDouble() < pitcher.StrikeoutBonus)
        {
            playByPlayLog.Add("[BONUS] Pitcher converts outcome into a strikeout due to accumulating K bonus.");
            return "K";
        }
        return result;
    }

    // --- Hit-By-Pitch Chance Layer ---
    private string MaybeApplyHBP(string result, Player pitcher, Player batter, string batterOption, string pitcherOption)
    {
        if (pitcherOption == "Intentional Walk") return result; // skip IW
        // Only allow HBP conversion on a walk outcome (BB); do not overwrite hits or outs or Ks
        if (result != "BB") return result;
        double baseChance = GetPitcherHBPBase(pitcher.ClassLevel);
        // Power hitters crowd plate slightly
        if (batterOption == "Power") baseChance += 0.005; // +0.5%
        if (batterOption == "Safe") baseChance -= 0.003;  // -0.3%
        baseChance = Math.Max(0, baseChance);
        return _rnd.NextDouble() < baseChance ? "HBP" : result;
    }
    private double GetPitcherHBPBase(string classLevel) => classLevel switch
    {
        "Freshman" => 0.025,
        "Sophomore" => 0.020,
        "Junior" => 0.015,
        "Senior" => 0.010,
        _ => 0.020
    };

    // --- Fielding & Error System Helpers ---
    private string ApplyFieldingErrorLayer(string result, Player pitcher, Player batter)
    {
        // Results that can trigger fielding checks
        string playType = result;
        bool isOutLike = result == "GO" || result == "FO" || result == "PO" || result == "PU" || result == "LO";
        bool isHitLike = IsHit(result);

        // Passed ball on strikeout possibility (out-of-position catcher)
        if (result == "K" && bases.AnyRunnerOnBase)
        {
            // 25% chance if catcher is out-of-position (simplified: random check)
            if (_rnd.NextDouble() < 0.25)
            {
                playByPlayLog.Add("[PASSED BALL] Batter reaches first on dropped third strike (K_PB).");
                // Advance actual batter to first; force runners forward only if occupied chain
                if (bases.First == null) bases.First = batter;
                else if (bases.Second == null) { bases.Second = bases.First; bases.First = batter; }
                else if (bases.Third == null) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter; }
                else { // bases loaded: score runner from 3rd, shift others, batter to first
                    ScoreRun();
                    bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter;
                }
                return "K_PB"; // distinct marker
            }
        }

    if (!isOutLike && !isHitLike) return result; // Only check fielding on outs or hits

    double errorChanceBase = 0; // Derived from random fielder's class
    Player? fielder = SelectRandomFielderForPlay(playType, pitcher);
        if (fielder == null) return result;
    errorChanceBase = GetClassErrorBase(fielder.ClassLevel) + fielder.ErrorAccumulator; // include accumulator

        // Out-of-position adjustments
        double penalty = GetOutOfPositionErrorAdjustment(fielder, playType);
        errorChanceBase += penalty;

        // Spectacular play chance equals base error chance (before penalty) + accumulator
        double spectacularChance = GetClassErrorBase(fielder.ClassLevel) + fielder.SpectacularPlayAccumulator;

        double rollVal = _rnd.NextDouble();
        if (rollVal < spectacularChance)
        {
            // Spectacular play converts hit to out or enhances out (double/triple play on grounder if runners)
            fielder.SpectacularPlayAccumulator += 0.005; // +0.50% future chance if no error inflation
            if (fielder.ErrorAccumulator >= 0.01) fielder.ErrorAccumulator = Math.Max(0, fielder.ErrorAccumulator - 0.01); // reduce +1% (0.01 decimal)
            if (isHitLike)
            {
                playByPlayLog.Add($"[SPECTACULAR] {fielder.Name} robs a hit!");
                return playType + "_ROB"; // mark robbed
            }
            if (playType == "GO" && bases.AnyRunnerOnBase)
            {
                int outsAdded = Math.Min(2, 3 - outs);
                outs += outsAdded; // enhance ground out
                playByPlayLog.Add($"[SPECTACULAR] {fielder.Name} turns a highlight-reel {outsAdded + 1}-out play!");
                return playType + "_DP";
            }
        }

        if (_rnd.NextDouble() < errorChanceBase)
        {
            playByPlayLog.Add($"[ERROR] {fielder.Name} commits an error on {playType}. Runner(s) advance.");
            fielder.ErrorAccumulator += 0.005; // +0.50% expressed as decimal
            // Convert out to single; upgrade single to error advancement
            if (isOutLike)
            {
                AdvanceOnHit("1B", batter, "");
                return playType + "_E"; // annotate
            }
            else if (isHitLike)
            {
                // Extra base on error
                AdvanceOnHit("2B", batter, "");
                return result + "_E";
            }
        }
        return result;
    }

    private Player? SelectRandomFielderForPlay(string playType, Player pitcher)
    {
        // Simplified: use pitching team's rotation first player as catcher surrogate & random roster sample
        Team? defenseTeam = currentTeamBatting == awayTeamName ? _engine.GetTeamByName(homeTeamName) : _engine.GetTeamByName(awayTeamName);
        if (defenseTeam == null || defenseTeam.Roster.Count == 0) return null;
        var pool = defenseTeam.Roster.ToList();
        // Narrow pool based on play type
        if (playType == "GO") pool = pool.Where(p => p.Positions.Any(IsInfield)).ToList();
        else if (playType == "BUNT") pool = pool.Where(p => p.Positions.Any(pos => pos == "3B" || pos == "P" || pos == "C" || pos == "1B")).ToList();
        else if (playType == "FO" || playType == "PO" || playType == "PU" || playType == "LO") pool = pool.Where(p => p.Positions.Any(IsInfield)).ToList();
        else if (playType == "2B" || playType == "3B" || playType == "HR" || playType == "1B") pool = pool.Where(p => p.Positions.Any(IsOutfield)).ToList();
        else if (playType == "K" || playType == "BB") pool = pool.Where(p => p.Positions.Any(pos => pos == "P" || pos == "C")).ToList();
        if (pool.Count == 0) pool = defenseTeam.Roster;
        return pool[_rnd.Next(pool.Count)];
    }
    private bool IsInfield(string pos) => pos is "1B" or "2B" or "3B" or "SS";
    private bool IsOutfield(string pos) => pos is "LF" or "CF" or "RF";
    private double GetClassErrorBase(string classLevel) => classLevel switch
    {
        "Freshman" => 0.09,
        "Sophomore" => 0.07,
        "Junior" => 0.05,
        "Senior" => 0.03,
        _ => 0.07
    };
    private double GetOutOfPositionErrorAdjustment(Player fielder, string playType)
    {
        // Determine if position involved matches one of player's positions; simplified due to lack of assigned fielding position context
        // Apply catcher special penalties if acting as catcher without "C" position
        bool isCatcherContext = playType == "K" || playType == "BB"; // approximate scenarios involving pitcher/catcher
        bool isCatcher = fielder.Positions.Any(p => p == "C");
        if (isCatcherContext && !isCatcher)
        {
            return 0.25; // +25% passed ball/error context
        }
        // General out-of-position (simplified): if no matching category
        bool hasInfield = fielder.Positions.Any(IsInfield);
        bool hasOutfield = fielder.Positions.Any(IsOutfield);
        if (playType == "GO" && !hasInfield) return 0.10;
        if ((playType == "2B" || playType == "3B" || playType == "HR" || playType == "1B") && !hasOutfield) return 0.10;
        // Intra-category leniency: infielders/outfielders switching within group smaller penalty
        if (playType == "GO" && hasInfield) return 0.05;
        if ((playType == "2B" || playType == "3B") && hasOutfield) return 0.05;
        return 0.0;
    }

    // --- Balk / Wild Pitch / Pickoff Helpers ---
    private void AttemptBalkOrWildPitch(Player pitcher)
    {
        double baseChance = GetPitcherBalkWildBase(pitcher.ClassLevel) + pitcher.BalkWildPitchAccumulator;
        double roll = _rnd.NextDouble();
        if (roll < baseChance)
        {
            // Decide balk vs wild pitch (50/50)
            bool isBalk = _rnd.NextDouble() < 0.5;
            if (isBalk)
            {
                playByPlayLog.Add("[BALK] All runners advance one base.");
                pitcher.BalkWildPitchAccumulator += 0.005;
                AdvanceAllRunnersOneBase();
            }
            else
            {
                playByPlayLog.Add("[WILD PITCH] Runners may advance (75% chance each)." );
                pitcher.BalkWildPitchAccumulator += 0.005;
                AdvanceRunnersOnWildPitch();
            }
        }
    }
    private double GetPitcherBalkWildBase(string classLevel) => classLevel switch
    {
        "Freshman" => 0.11,
        "Sophomore" => 0.09,
        "Junior" => 0.07,
        "Senior" => 0.05,
        _ => 0.09
    };
    private void AdvanceAllRunnersOneBase()
    {
        ScoreAndClear(ref bases.Third);
        bases.Third = bases.Second;
        bases.Second = bases.First;
        bases.First = null;
    }
    private void AdvanceRunnersOnWildPitch()
    {
        // Each runner individually 75% chance
        if (bases.Third != null && _rnd.NextDouble() < 0.75) { ScoreAndClear(ref bases.Third); }
        if (bases.Second != null && _rnd.NextDouble() < 0.75) { bases.Third = bases.Second; bases.Second = null; }
        if (bases.First != null && _rnd.NextDouble() < 0.75)
        {
            if (bases.Second == null) { bases.Second = bases.First; bases.First = null; }
            else if (bases.Third == null) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
        }
    }
    private void AttemptPickoff(Player pitcher)
    {
        // Enhanced pickoff selection per new rules: choose occupied base randomly.
        var occupied = new System.Collections.Generic.List<int>();
        if (bases.First != null) occupied.Add(1);
        if (bases.Second != null) occupied.Add(2);
        if (bases.Third != null) occupied.Add(3);
        if (occupied.Count == 0) return;
        double baseChance = GetPickoffBase(pitcher.ClassLevel) + pitcher.PickoffAccumulator;
        if (_rnd.NextDouble() >= baseChance) return;
        int chosen;
        if (occupied.Count == 1) chosen = occupied[0];
        else
        {
            // Roll die equal to number of occupied bases
            chosen = occupied[_rnd.Next(occupied.Count)];
        }
        // Apply out; special rule: if runner on 2nd is picked off (<2 outs) 30% chance runner on 3rd scores
        if (chosen == 1) { playByPlayLog.Add("[PICKOFF] Runner on 1st is picked off!"); bases.First = null; }
        else if (chosen == 2) {
            playByPlayLog.Add("[PICKOFF] Runner on 2nd is picked off!"); bases.Second = null; 
            if (bases.Third != null && outs < 2 && _rnd.NextDouble() < 0.30)
            {
                playByPlayLog.Add("[PICKOFF BONUS] Runner on 3rd dashes home and scores during pickoff confusion!");
                ScoreAndClear(ref bases.Third);
            }
        }
        else if (chosen == 3) { playByPlayLog.Add("[PICKOFF] Runner on 3rd is picked off!"); bases.Third = null; }
        pitcher.PickoffAccumulator += 0.005; outs++;
    }
    private double GetPickoffBase(string classLevel) => classLevel switch
    {
        "Freshman" => 0.03,
        "Sophomore" => 0.05,
        "Junior" => 0.07,
        "Senior" => 0.09,
        _ => 0.05
    };

    // --- Pitch Count / Fatigue Mechanics ---
    private enum OutcomePitchCost { BattedOut, Hit, Strikeout, Walk }
    private PitcherState GetPitcherState(Player pitcher)
    {
        if (!_pitcherStates.TryGetValue(pitcher.Name, out var st))
        {
            int baseRating = GetBasePitchingRating(pitcher);
            st = new PitcherState { MaxPitchCount = baseRating * 10, CurrentPitches = 0, StrikeoutsThisInning = 0, RunsThisInning = 0, RatingPenalty = 0 };
            _pitcherStates[pitcher.Name] = st;
        }
        return st;
    }
    private void AddPitchCount(Player pitcher, OutcomePitchCost cost)
    {
        var st = GetPitcherState(pitcher);
        int add = cost switch
        {
            OutcomePitchCost.BattedOut => _rnd.Next(1,7), // d6
            OutcomePitchCost.Hit => _rnd.Next(1,9),       // d8
            OutcomePitchCost.Strikeout => 3 + _rnd.Next(1,4), // 3 + d3
            OutcomePitchCost.Walk => 4 + _rnd.Next(1,3),      // 4 + d2
            _ => 1
        };
        st.CurrentPitches += add;
    }
    private void AddPitcherStrikeoutBonus(Player pitcher)
    {
        var st = GetPitcherState(pitcher);
        st.CurrentPitches = Math.Max(0, st.CurrentPitches - 3); // treat as +3 to capacity equivalently by reducing count
    }
    private void AddPitcherScorelessBonus(Player pitcher)
    {
        var st = GetPitcherState(pitcher);
        st.CurrentPitches = Math.Max(0, st.CurrentPitches - 10);
    }
    private int GetCurrentInningRunsAgainst(Player pitcher)
    {
        // Simplified: use playByPlay tags; here we don't track per-pitcher runs, so return 0 to allow bonus only when outs end and no score changes in this half-inning
        return 0;
    }

    // --- Base Running Helpers (simplified) ---
    private void AdvanceOnWalk(Player batter)
    {
        // Force chain: only push a runner if the base behind is occupied
        if (bases.First != null)
        {
            if (bases.Second != null)
            {
                if (bases.Third != null)
                {
                    // Bases loaded: force in a run
                    ScoreRun();
                }
                // Push 2nd to 3rd
                if (bases.Third == null) bases.Third = bases.Second;
                // Push 1st to 2nd
                bases.Second = bases.First;
            }
            else
            {
                // Only 1st occupied: push to 2nd
                bases.Second = bases.First;
            }
            bases.First = batter;
        }
        else
        {
            // First base open
            bases.First = batter;
        }
    }
    private void AdvanceOnHit(string res, Player batter, string pitcherOption)
    {
        int basesToAdvance = res switch { "1B" => 1, "2B" => 2, "3B" => 3, "HR" => 4, _ => 0 };
    if (res == "FO_SF") { ScoreAndClear(ref bases.Third); return; }
        if (res == "GO_DP") { // double play simplistic: runner on first out and batter out
            outs = Math.Min(3, outs + 2);
            if (bases.First != null) bases.First = null; // remove lead runner on first
            return;
        }
        if (basesToAdvance == 0) return;
        // Move existing runners
        for (int step = 0; step < basesToAdvance; step++)
        {
            ScoreAndClear(ref bases.Third);
            bases.Third = bases.Second;
            bases.Second = bases.First;
            bases.First = null;
        }
        if (basesToAdvance < 4)
        {
            // place batter
            if (basesToAdvance == 1) bases.First = batter;
            else if (basesToAdvance == 2) bases.Second = batter;
            else if (basesToAdvance == 3) bases.Third = batter;
        }
        else
        {
            // HR
            ScoreRun();
        }
    }
    private void ScoreRun()
    {
        if (currentTeamBatting == homeTeamName) scoreHome++; else scoreAway++;
    }

    // --- Common advancement utilities (refactor duplication) ---
    private void ScoreAndClear(ref Player? runner)
    {
        if (runner != null)
        {
            ScoreRun();
            runner = null;
        }
    }

    private void ScoreAllOccupiedBases()
    {
        ScoreAndClear(ref bases.Third);
        ScoreAndClear(ref bases.Second);
        ScoreAndClear(ref bases.First);
    }

    private void ResolveSteal(string pitcherOption)
    {
        // Choose lead runner to attempt steal (prefer 1st -> 2nd)
        if (bases.First == null) { playByPlayLog.Add("[STEAL] No runner on first to steal."); return; }
        int runnerRating = bases.First.BattingRating + bases.First.ClassModifier;
        int defenseRating = 0; // simplified catcher/pitcher lowest rating not tracked; approximate using 6
        int runnerRoll = _rnd.Next(1,7) + runnerRating;
        int defenseRoll = _rnd.Next(1,7) + defenseRating;
        if (pitcherOption == "Pitch Out") defenseRoll += (int)Math.Round(0.75 * 6); // crude boost
        if (runnerRoll >= defenseRoll)
        {
            playByPlayLog.Add("[STEAL] Runner steals second!");
            bases.Second = bases.First; bases.First = null;
        }
        else
        {
            playByPlayLog.Add("[STEAL] Runner thrown out!");
            outs++;
        }
    }
    private void ResolveDoubleSteal(string pitcherOption)
    {
        // Lead runner then trailing runner
        if (bases.Second == null) { ResolveSteal(pitcherOption); return; }
        var savedFirst = bases.First; var savedSecond = bases.Second;
        bases.First = savedFirst; bases.Second = savedSecond;
        // Lead runner
        int leadRating = savedSecond.BattingRating + savedSecond.ClassModifier;
        int def = 0;
        int leadRoll = _rnd.Next(1,7) + leadRating;
        int defRoll = _rnd.Next(1,7) + def;
        if (pitcherOption == "Pitch Out") defRoll += (int)Math.Round(0.75 * 6);
        if (leadRoll >= defRoll) { playByPlayLog.Add("[DOUBLE STEAL] Lead runner advances!"); bases.Third = savedSecond; bases.Second = savedFirst; bases.First = null; }
        else { playByPlayLog.Add("[DOUBLE STEAL] Lead runner out!"); outs++; bases.Second = savedFirst; bases.First = null; return; }
        // Trailing runner
        if (bases.Second != null)
        {
            int tailRating = bases.Second.BattingRating + bases.Second.ClassModifier;
            int tailRoll = _rnd.Next(1,7) + tailRating;
            int tailDefRoll = _rnd.Next(1,7) + def;
            if (tailRoll >= tailDefRoll) { playByPlayLog.Add("[DOUBLE STEAL] Trailing runner advances!"); bases.Third = bases.Second; bases.Second = null; }
            else { playByPlayLog.Add("[DOUBLE STEAL] Trailing runner out!"); outs++; bases.Second = null; }
        }
    }

    // --- Suicide Squeeze Resolution ---
    private bool ResolveSuicideSqueeze(Player batter, Player pitcher, string pitcherOption)
    {
        // Pitch Out counter scenario: runner on 3rd attempts to retreat (10% safe)
        if (pitcherOption == "Pitch Out")
        {
            if (_rnd.NextDouble() < 0.10)
            {
                playByPlayLog.Add("[SUICIDE SQUEEZE] Pitch out called; runner scrambles back to 3rd safely (10% success). Play resets.");
                return false; // continue with normal at-bat
            }
            playByPlayLog.Add("[SUICIDE SQUEEZE] Pitch out thwarts squeeze; runner from 3rd tagged out!");
            bases.Third = null; outs++; AddPitchCount(pitcher, OutcomePitchCost.BattedOut); if (outs == 3) EndHalfInning(); return true;
        }
        double successChance = 0.60;
        if (pitcherOption == "Infield In") successChance -= 0.20; // tighter defense
        bool success = _rnd.NextDouble() < successChance;
        if (success)
        {
            int roll = RollDie(20);
            string narrative = GetSqueezeSuccessNarrative(roll);
            playByPlayLog.Add($"[SUICIDE SQUEEZE SUCCESS] {narrative}");
            // Runner scores
            ScoreAndClear(ref bases.Third);
            // Batter safe logic depending on narrative (some outcomes batter out at first)
            bool batterOut = narrative.Contains("thrown out at first") || narrative.Contains("takes the out at first") || narrative.Contains("batter out at first") || narrative.Contains("batter thrown out at first");
            if (batterOut) { outs++; AddPitchCount(pitcher, OutcomePitchCost.BattedOut); }
            else { AdvanceOnHit("1B", batter, ""); AddPitchCount(pitcher, OutcomePitchCost.Hit); }
            if (outs == 3) EndHalfInning();
            return true;
        }
        // Failure chart (d10) especially harsh if Infield In
        int failRoll = RollDie(10);
        string failNarrative = GetSqueezeFailNarrative(failRoll);
        playByPlayLog.Add($"[SUICIDE SQUEEZE FAIL] {failNarrative}");
        // Interpret failures
        if (failNarrative.Contains("runner out") || failNarrative.Contains("tagged out") || failNarrative.Contains("caught"))
        {
            // Runner from 3rd out
            if (bases.Third != null) { bases.Third = null; outs++; }
        }
        if (failNarrative.Contains("double play")) {
            outs = Math.Min(3, outs + 1); // already added one
        }
        // Batter out on foul third strike or popup
        if (failNarrative.Contains("foul on third strike") || failNarrative.Contains("pop-up")) outs++;
        AddPitchCount(pitcher, OutcomePitchCost.BattedOut);
        if (outs >= 3) EndHalfInning();
        return true;
    }

    private string GetSqueezeSuccessNarrative(int roll) => roll switch
    {
        1 => "Perfect bunt toward 1st base line, pitcher late — run scores standing.",
        2 => "Soft drop between mound and 3rd; throw to first as runner slides home.",
        3 => "Deadens in front of plate; catcher hesitates — batter beats it, run scores.",
        4 => "Toward 3B, fielder charges; late home throw — both safe.",
        5 => "Push bunt up 1B line; easy out recorded while run scores.",
        6 => "Surprise squeeze perfectly placed — RBI bunt.",
        7 => "High bounce; catcher waits — runner slides under tag.",
        8 => "Soft near mound; pitcher bobbles — all safe, run in.",
        9 => "Drag past pitcher; 2B too deep — batter safe, run scores.",
        10 => "Straight back to pitcher; off-target home throw — run scores, batter safe.",
        11 => "Harder bunt to 3B; run scores, batter thrown out at first.",
        12 => "Popped slightly, drops in no-man's land — close, run scores.",
        13 => "Fielded cleanly; home throw just late — run slides under tag.",
        14 => "Pitch outside; reach bunt foul — runner scrambles back (play continues).",
        15 => "Low & inside, batter pulls back — catcher blocks, runner caught off third!",
        16 => "Toward 1B; fielder chooses out at first — run scores.",
        17 => "Up & in; awkward bunt still plates run.",
        18 => "Down 3B line; pitcher covers late — run scores.",
        19 => "Miss-hit bunt; runner hesitates; defense can't recover — run in.",
        20 => "Weak bunt; double-clutch throw home — runner beats it.",
        _ => "Routine squeeze, run scores."
    };
    private string GetSqueezeFailNarrative(int roll) => roll switch
    {
        1 => "Missed bunt; runner in rundown and tagged out.",
        2 => "Pop-up to catcher — double play off third.",
        3 => "Foul bunt on third strike; batter out, runner retreats.",
        4 => "Pitchout; easy tag at home.",
        5 => "Back to pitcher; quick home throw — runner out, batter safe at first.",
        6 => "Too hard to 3B; fired home — runner tagged out.",
        7 => "Missed sign; runner hung up and tagged.",
        8 => "High pitch; batter whiffs — runner tagged at plate.",
        9 => "Pop-up to 1B; nearly doubles off runner at third.",
        10 => "Down 1B line; catcher flips home — runner out.",
        _ => "Failed squeeze; runner holds." 
    };

    // --- Sacrifice Fly Resolution ---
    private void ResolveSacrificeFly(Player batter)
    {
        // Determine base configuration
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        int roll = RollDie(10);
        if (on3 && on1 && !on2)
        {
            // Runners on 3rd and 1st
            SacFlyRunnersThirdAndFirst(roll);
        }
        else if (on2 && on1 && !on3)
        {
            // Runners on 2nd and 1st (advance lead to 3rd typically)
            SacFlyRunnersSecondAndFirst(roll);
        }
        else if (on1 && on2 && on3)
        {
            SacFlyBasesLoaded(roll);
        }
        else if (on3 && !on1 && !on2)
        {
            // Simple sac fly scoring runner from third
            playByPlayLog.Add("[SAC FLY] Deep enough, runner on 3rd tags and scores."); ScoreAndClear(ref bases.Third);
        }
        else
        {
            playByPlayLog.Add("[SAC FLY] Caught, no complex tag scenario."); if (on3) { ScoreAndClear(ref bases.Third); }
        }
        outs++;
    }

    // --- Sacrifice Bunt Resolution (generic) ---
    private void ResolveSacrificeBunt(Player batter)
    {
        // Simple generic d10 narrative; advance lead forced runner(s) one base, batter out
        int r = RollDie(10);
        string narrative = r switch
        {
            1 => "Squares early, gets it down toward first — sacrifice successful.",
            2 => "Deadens in front, catcher fields and takes the out — runners advance.",
            3 => "Perfect push toward third — only play is at first.",
            4 => "Bunted up the line, pitcher covers and gets the out — job done.",
            5 => "Drops it between mound and third — sacrifice executed.",
            6 => "Chopped bunt, slow roller — throw to first in time, runners move up.",
            7 => "Bunts toward second, fielder charges — gets the sure out, advance for runners.",
            8 => "Well-placed bunt, defense has no shot but at first.",
            9 => "Drag bunt toward first; 1B takes it and steps on the bag — runners advance.",
            _ => "Small-ball executed — sacrifice bunt successful."
        };
        playByPlayLog.Add($"[SAC BUNT] {narrative}");
        // Advance forced runners one base
        if (bases.Third != null && bases.Second != null && bases.First != null)
        {
            // Bases loaded: force in a run
            ScoreAndClear(ref bases.Third); // replaced with helper
            bases.Third = bases.Second; bases.Second = bases.First; bases.First = null;
        }
        else
        {
            if (bases.Second != null && bases.Third == null) { bases.Third = bases.Second; bases.Second = null; }
            if (bases.First != null && bases.Second == null) { bases.Second = bases.First; bases.First = null; }
        }
        // Batter is out at first
        outs++;
    }
    private void SacFlyRunnersThirdAndFirst(int roll)
    {
        switch (roll)
        {
            case 1: playByPlayLog.Add("[SAC FLY] Deep R to R; run scores, runner from 1st takes 2nd."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
            case 2: playByPlayLog.Add("[SAC FLY] LF; run scores standing; runner on 1st holds."); ScoreAndClear(ref bases.Third); break;
            case 3: playByPlayLog.Add("[SAC FLY] CF; both tag, one scores, runner to 2nd."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
            case 4: playByPlayLog.Add("[SAC FLY] R-CF cutoff; 1st runner advances."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
            case 5: playByPlayLog.Add("[SAC FLY] Shallow RF; runner bluffs, no advance."); ScoreAndClear(ref bases.Third); break;
            case 6: playByPlayLog.Add("[SAC FLY] Deep L-CF; both advance, one run."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
            case 7: playByPlayLog.Add("[SAC FLY] Medium LF; run scores, runner holds."); ScoreAndClear(ref bases.Third); break;
            case 8: playByPlayLog.Add("[SAC FLY] Deep RF; errant cutoff, runner from 1st takes 2nd."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
            case 9: playByPlayLog.Add("[SAC FLY] Near RF foul pole; tag & score, runner holds."); ScoreAndClear(ref bases.Third); break;
            case 10: playByPlayLog.Add("[SAC FLY] CF off-line throw; run scores, runner to 2nd."); ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; break;
        }
    }
    private void SacFlyRunnersSecondAndFirst(int roll)
    {
        switch (roll)
        {
            case 1: playByPlayLog.Add("[SAC FLY] Deep RF; runner on 2nd tags to 3rd, 1st holds."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 2: playByPlayLog.Add("[SAC FLY] CF; both tag & advance."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 3: playByPlayLog.Add("[SAC FLY] LF; only lead runner advances."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 4: playByPlayLog.Add("[SAC FLY] Deep L-CF; both advance without play."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 5: playByPlayLog.Add("[SAC FLY] Shallow RF; both hold."); break;
            case 6: playByPlayLog.Add("[SAC FLY] R-CF; lead runner advances."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 7: playByPlayLog.Add("[SAC FLY] Deep LF; both tag; lead to 3rd, trail to 2nd."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 8: playByPlayLog.Add("[SAC FLY] RF misplay choice; both advance."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 9: playByPlayLog.Add("[SAC FLY] Deep CF; both advance easily."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 10: playByPlayLog.Add("[SAC FLY] Medium LF; only runner on 2nd advances."); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
        }
    }
    private void SacFlyBasesLoaded(int roll)
    {
        switch (roll)
        {
            case 1: playByPlayLog.Add("[SAC FLY] Deep R-CF; run scores; others tag one base."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 2: playByPlayLog.Add("[SAC FLY] LF throw home; run safe, others hold."); ScoreAndClear(ref bases.Third); break;
            case 3: playByPlayLog.Add("[SAC FLY] Deep CF; run scores; runner from 2nd to 3rd."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 4: playByPlayLog.Add("[SAC FLY] RF strong throw; run scores; trailing runners advance."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 5: playByPlayLog.Add("[SAC FLY] L-CF gap caught; one run; others stay."); ScoreAndClear(ref bases.Third); break;
            case 6: playByPlayLog.Add("[SAC FLY] Deep LF; run in; runner from 2nd holds."); ScoreAndClear(ref bases.Third); break;
            case 7: playByPlayLog.Add("[SAC FLY] Deep CF; run scores; others advance into scoring position."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 8: playByPlayLog.Add("[SAC FLY] RF throw misses cutoff; run in; bases reloaded."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            case 9: playByPlayLog.Add("[SAC FLY] Shallow LF bluff; no advance."); break;
            case 10: playByPlayLog.Add("[SAC FLY] Deep R-CF; run scores; two runners advance."); ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
        }
    }

    // --- Dice & Narrative Helpers ---
    private int RollDie(int sides) => _rnd.Next(1, sides + 1);
    

    // --- Double Narratives & Advancement ---
    private void ResolveDoubleWithRunners(Player batter, string batterOption, string pitcherOption)
    {
        bool noDoubles = string.Equals(pitcherOption, "No Doubles", StringComparison.OrdinalIgnoreCase);
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        int r = RollDie(10);
        if (!on1 && !on2 && !on3)
        {
            // Bases empty double
            string narrative = NarrativeHelpers.GetDoubleNoRunnersNarrative(r, noDoubles);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Downgraded to single
                bases.First = batter;
                return;
            }
            // Place batter at second
            bases.Second = batter;
        }
        else if (on1 && !on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetDoubleR1Narrative(r, safe, noDoubles);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single; runner goes to 3rd per chart; batter to 1st
                bases.Third = bases.First; bases.First = batter; return;
            }
            ApplyDoubleRunnerOnFirstAdvancement(r, safe);
            // Batter to second
            bases.Second = batter;
        }
        else if (!on1 && on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetDoubleR2Narrative(r, safe, noDoubles);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: runner from 2nd to 3rd only
                bases.Third = bases.Second; bases.Second = null; bases.First = batter; return;
            }
            ApplyDoubleRunnerOnSecondAdvancement(r, safe);
            // Batter to second
            bases.Second = batter;
        }
        else if (!on1 && !on2 && on3)
        {
            string narrative = NarrativeHelpers.GetDoubleR3Narrative(r, safe, noDoubles, pitcherOption);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: run scores, batter to first
                ScoreAndClear(ref bases.Third); bases.First = batter; return;
            }
            ApplyDoubleRunnerOnThirdAdvancement(r, safe);
            // Batter to second
            bases.Second = batter;
        }
        else if (on1 && on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetDoubleR1R2Narrative(r, safe, noDoubles);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: both advance one base
                bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter; return;
            }
            ApplyDoubleRunnersOnFirstSecondAdvancement(r, safe);
            // Batter to second
            bases.Second = batter;
        }
        else if (on1 && !on2 && on3)
        {
            // Runners on 1st and 3rd
            string narrative = NarrativeHelpers.GetDoubleR1R3Narrative(r, safe, noDoubles, pitcherOption);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: R3 scores, R1 to 3rd or 2nd per conservative send; favor to 3rd unless Safe
                ScoreAndClear(ref bases.Third);
                if (safe) { bases.Second = bases.First; bases.First = batter; }
                else { bases.Third = bases.First; bases.First = batter; }
                return;
            }
            ApplyDoubleRunnersOnFirstThirdAdvancement(r, safe, pitcherOption);
            // Batter to second
            bases.Second = batter;
        }
        else if (!on1 && on2 && on3)
        {
            // Runners on 2nd and 3rd
            string narrative = NarrativeHelpers.GetDoubleR2R3Narrative(r, safe, noDoubles, pitcherOption);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: R3 scores; R2 to 3rd; batter to 1st
                ScoreAndClear(ref bases.Third); bases.Third = bases.Second; bases.Second = null; bases.First = batter; return;
            }
            ApplyDoubleRunnersOnSecondThirdAdvancement(r, safe, pitcherOption);
            bases.Second = batter;
        }
        else if (on1 && on2 && on3)
        {
            // Bases loaded
            string narrative = NarrativeHelpers.GetDoubleBasesLoadedNarrative(r, safe, noDoubles, pitcherOption);
            playByPlayLog.Add($"[DOUBLE NARRATIVE] {narrative}");
            if (noDoubles)
            {
                // Single: one run scores; others advance one
                ScoreAndClear(ref bases.Third); bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter; return;
            }
            ApplyDoubleBasesLoadedAdvancement(r, safe, pitcherOption);
            bases.Second = batter;
        }
        else
        {
            // For now, fallback to generic advancement
            playByPlayLog.Add("[DOUBLE NARRATIVE] Generic advancement applied (charts for this configuration not yet defined)." );
            AdvanceOnHit("2B", batter, pitcherOption);
        }
    }
    
    private void ApplyDoubleRunnerOnFirstAdvancement(int roll, bool safe)
    {
        // Determine if runner from first scores or holds at 3rd
        bool runnerScores = roll is 1 or 2 or 3 or 4 or 5 or 6 or 7 or 9; // 8 & 10 explicitly first-to-third only
        if (safe && runnerScores) runnerScores = roll is 3 or 6 ? true : false; // Some too deep to stop
        if (runnerScores)
        {
            ScoreRun(); bases.First = null; // Runner from first scores on double (no third-base clear needed here)
        }
        else
        {
            bases.Third = bases.First; bases.First = null;
        }
        // Batter placement handled by caller
    }
    
    private void ApplyDoubleRunnerOnSecondAdvancement(int roll, bool safe)
    {
        bool scores = roll switch { 3 => !safe, 4 => !safe, 6 => !safe, 9 => false, _ => true };
    if (scores) { ScoreRun(); bases.Second = null; } else { bases.Third = bases.Second; bases.Second = null; }
        // Batter placed at second by caller
    }
    
    private void ApplyDoubleRunnerOnThirdAdvancement(int roll, bool safe)
    {
        bool runScores = roll != 3; // roll 3 may hold with OF in handled earlier
    if (runScores) { ScoreAndClear(ref bases.Third); }
        // Batter to second handled by caller
    }
    
    private void ApplyDoubleRunnersOnFirstSecondAdvancement(int roll, bool safe)
    {
        bool leadScores = roll switch { 2 => !safe, 3 => !safe, 4 => true, 5 => true, 1 => !safe, 6 => !safe, _ => true };
        bool trailToThird = roll switch { 1 => true, 2 => !safe, 3 => !safe, 4 => true, 5 => !safe, 6 => !safe, _ => true };
        if (leadScores)
        {
            ScoreRun(); bases.Second = null;
        }
        else
        {
            bases.Third = bases.Second; bases.Second = null;
        }
        if (trailToThird)
        {
            bases.Third = bases.Third ?? bases.First; // if lead didn't move to third, trail might
            if (bases.First != null && bases.Third == bases.First) bases.First = null; // avoid duplicate ref
        }
        else
        {
            bases.Second = bases.First; bases.First = null;
        }
        // Batter ends at second handled by caller
    }

    
    private void ApplyDoubleRunnersOnFirstThirdAdvancement(int roll, bool safe, string pitcherOption)
    {
        // Base outcomes: R3 usually scores; R1 to 3rd; sometimes both score on aggressive sends
        bool r3Scores = roll != 3 || pitcherOption != "Outfield In"; // on 3 with OF in, hold
        bool r1Scores = roll is 2 or 5 or 7 or 10 && !safe; // aggressive cases only when not Safe
    if (r3Scores) { ScoreAndClear(ref bases.Third); }
        if (r1Scores)
        {
            ScoreRun(); bases.First = null;
        }
        else
        {
            bases.Third = bases.First; bases.First = null;
        }
        // Batter at second by caller
    }

    
    private void ApplyDoubleRunnersOnSecondThirdAdvancement(int roll, bool safe, string pitcherOption)
    {
        bool twoScore = roll switch { 2 => !safe, 3 => pitcherOption != "Outfield In", 6 => !safe, 9 => !safe, _ => true };
        if (twoScore)
        {
            // Both score
            if (bases.Third != null) { ScoreAndClear(ref bases.Third); }
            if (bases.Second != null) { ScoreRun(); bases.Second = null; }
        }
        else
        {
            // Only R3 scores; R2 to 3rd
            if (bases.Third != null) { ScoreAndClear(ref bases.Third); }
            bases.Third = bases.Second; bases.Second = null;
        }
        // Batter at second by caller
    }

    
    private void ApplyDoubleBasesLoadedAdvancement(int roll, bool safe, string pitcherOption)
    {
        // Typically two runs score on a double with BL; Safe or OF In can limit to one
        bool twoScore = roll switch { 1 => !safe, 2 => !safe, 3 => pitcherOption != "Outfield In", 6 => !safe, 9 => !safe, _ => true };
        if (twoScore)
        {
            // Score R3 and R2; R1 to 3rd
            if (bases.Third != null) { ScoreAndClear(ref bases.Third); }
            if (bases.Second != null) { ScoreRun(); bases.Second = null; }
            bases.Third = bases.First; bases.First = null;
        }
        else
        {
            // Only one scores; others advance one
            if (bases.Third != null) { ScoreAndClear(ref bases.Third); }
            bases.Third = bases.Second; bases.Second = bases.First; bases.First = null;
        }
        // Batter to second by caller
    }

    // --- Triple Narratives & Advancement ---
    private void ResolveTripleWithRunners(Player batter, string batterOption, string pitcherOption)
    {
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        int r = RollDie(10);
        if (!on1 && !on2 && !on3)
        {
            // Solo triple description
            string narrative = NarrativeHelpers.GetTripleNoRunnersNarrative(r);
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {narrative}");
            bases.Third = batter; return;
        }
        else if (on1 && !on2 && !on3)
        {
            // Runner on 1st: scores, batter to 3rd
            string narrative = NarrativeHelpers.GetTripleR1Narrative(r);
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {narrative}");
            ScoreAndClear(ref bases.First);
            bases.Third = batter; return;
        }
        else if (!on1 && on2 && !on3)
        {
            // Runner on 2nd: scores, batter to 3rd
            string narrative = NarrativeHelpers.GetTripleR2Narrative(r);
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {narrative}");
            ScoreAndClear(ref bases.Second);
            bases.Third = batter; return;
        }
        else if (!on1 && !on2 && on3)
        {
            // Runner on 3rd: scores, batter to 3rd
            string narrative = NarrativeHelpers.GetTripleR3Narrative(r);
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {narrative}");
            ScoreAndClear(ref bases.Third);
            bases.Third = batter; return;
        }
        else if (on1 && on2 && !on3)
        {
            // Runners on 1st & 2nd: both score, batter to 3rd
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {NarrativeHelpers.GetTripleR1R2Narrative(r)}");
            ScoreAllOccupiedBases();
            bases.Third = batter; return;
        }
        else if (on1 && !on2 && on3)
        {
            // Runners on 1st & 3rd: both score, batter to 3rd
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {NarrativeHelpers.GetTripleR1R3Narrative(r)}");
            ScoreAllOccupiedBases();
            bases.Third = batter; return;
        }
        else if (!on1 && on2 && on3)
        {
            // Runners on 2nd & 3rd: both score, batter to 3rd
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {NarrativeHelpers.GetTripleR2R3Narrative(r)}");
            ScoreAllOccupiedBases();
            bases.Third = batter; return;
        }
        else if (on1 && on2 && on3)
        {
            // Bases loaded: all score, batter to 3rd
            playByPlayLog.Add($"[TRIPLE NARRATIVE] {NarrativeHelpers.GetTripleBasesLoadedNarrative(r)}");
            ScoreAllOccupiedBases();
            bases.Third = batter; return;
        }
        else
        {
            // Generic: all existing runners score on a triple; batter to third
            ScoreAllOccupiedBases();
            bases.Third = batter; return;
        }
    }

    // --- Home Run Narratives & Scoring ---
    private void ResolveHomeRun(Player batter)
    {
        int runnersOn = 0;
        if (bases.First != null) runnersOn++;
        if (bases.Second != null) runnersOn++;
        if (bases.Third != null) runnersOn++;

        string narrative = NarrativeHelpers.GetHomeRunNarrative(runnersOn);

        // Log and score
        playByPlayLog.Add($"[HOME RUN] {narrative}");

        // Score existing runners
        ScoreAllOccupiedBases();
        // Score batter
        ScoreRun();
    }

    // --- Walk Narratives ---
    private void ResolveWalk(Player batter)
    {
        string narrative = NarrativeHelpers.GetWalkNarrative();
        playByPlayLog.Add($"[WALK] {narrative}");
        AdvanceOnWalk(batter);
    }

    // --- Hit By Pitch Narratives ---
    private void ResolveHitByPitch(Player batter)
    {
        string narrative = NarrativeHelpers.GetHbpNarrative();
        playByPlayLog.Add($"[HBP] {narrative}");
        // Same advancement as walk
        AdvanceOnWalk(batter);
    }

    // --- Single Advancement with Runners ---
    private void ResolveSingleWithRunners(Player batter, string batterOption, string pitcherOption)
    {
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        bool outfieldIn = string.Equals(pitcherOption, "Outfield In", StringComparison.OrdinalIgnoreCase);
        bool infieldIn = string.Equals(pitcherOption, "Infield In", StringComparison.OrdinalIgnoreCase);
        bool dpDepth = string.Equals(pitcherOption, "Double Play Depth", StringComparison.OrdinalIgnoreCase);

        int roll = RollDie(10);
        if (on1 && !on2 && !on3)
        {
            playByPlayLog.Add($"[1B, R1] {NarrativeHelpers.GetSingleR1Narrative(roll)}");
            // Advancement identical to original switch
            if (roll is 1 or 4 or 7 or 8 or 10) { bases.Third = bases.First; bases.First = null; }
            else { bases.Second = bases.First; bases.First = null; }
            // Batter ends up at 1st
            bases.First = batter;
        }
        else if (!on1 && on2 && !on3)
        {
            playByPlayLog.Add($"[1B, R2] {NarrativeHelpers.GetSingleR2Narrative(roll, safe, infieldIn, outfieldIn)}");
            if (roll is 1 or 3 or 4 or 5 or 6 or 7 or 9 or 10 || safe || infieldIn || outfieldIn) { // hold patterns move runner to 3rd only
                if (roll == 1 && !(safe || infieldIn || outfieldIn)) { ScoreRun(); bases.Second = null; } else { bases.Third = bases.Second; bases.Second = null; }
            } else { ScoreRun(); bases.Second = null; }
            // Batter to 1st
            bases.First = batter;
        }
        else if (!on1 && !on2 && on3)
        {
            playByPlayLog.Add($"[1B, R3] {NarrativeHelpers.GetSingleR3Narrative(roll, outfieldIn)}");
            if (roll is 1 or 2 or 4 or 5 or 6 or 7 or 8 or 10 || (!outfieldIn && (roll == 3 || roll == 9))) { ScoreAndClear(ref bases.Third); } else { /* hold */ }
            bases.First = batter;
        }
        else if (on1 && on2 && !on3)
        {
            playByPlayLog.Add($"[1B, R1R2] {NarrativeHelpers.GetSingleR1R2Narrative(roll, safe, infieldIn, outfieldIn)}");
            if (roll == 4 && !safe) { ScoreAndClear(ref bases.Third); bases.Third = bases.First; bases.Second = null; bases.First = null; }
            else if (roll == 9 && !(safe || outfieldIn)) { ScoreAndClear(ref bases.Third); bases.Third = bases.First; bases.Second = null; bases.First = null; }
            else if (roll == 1 || roll == 2 || roll == 3 || roll == 5 || roll == 6 || roll == 8 || roll == 10 || safe || infieldIn || outfieldIn) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else { bases.Second = bases.First; bases.First = null; }
            bases.First ??= batter; // typically batter at 1st
        }
        else if (on1 && !on2 && on3)
        {
            playByPlayLog.Add($"[1B, R1R3] {NarrativeHelpers.GetSingleR1R3Narrative(roll, safe, infieldIn, dpDepth, outfieldIn)}");
            if (roll == 9 && (infieldIn || dpDepth)) { bases.Third = null; outs++; bases.Second = bases.First; bases.First = null; }
            else if (roll == 8 && !safe) { ScoreAndClear(ref bases.Third); bases.Third = bases.First; bases.First = null; }
            else if (roll == 6 && !(infieldIn || dpDepth)) { ScoreAndClear(ref bases.Third); bases.Second = bases.First; bases.First = null; }
            else if (roll == 1 || roll == 2 || roll == 3 || roll == 4 || roll == 5 || roll == 7 || roll == 10 || (roll == 8 && safe) || roll == 6) {
                if (!(infieldIn || dpDepth) && (roll == 1 || roll == 2 || roll == 3 || roll == 4 || roll == 5 || roll == 7 || roll == 10 || (roll == 6 && !(infieldIn||dpDepth)) || (roll==8 && !safe))) { ScoreRun(); }
                if (bases.Third != null && roll != 9) bases.Third = null;
                bases.Second = bases.First; bases.First = null; }
            bases.First ??= batter;
        }
        else if (!on1 && on2 && on3)
        {
            playByPlayLog.Add($"[1B, R2R3] {NarrativeHelpers.GetSingleR2R3Narrative(roll, safe, infieldIn, dpDepth, outfieldIn)}");
            bool bothScore = !(infieldIn||dpDepth) && !outfieldIn && (roll == 2 || roll ==5 || roll ==9) ? true : (roll ==2 && !(infieldIn||dpDepth)) || (roll==5 && !(infieldIn||dpDepth)) || (roll==9 && !outfieldIn);
            if (roll==2 && (infieldIn||dpDepth)) { bases.Third = bases.Second; bases.Second = null; }
            else if (roll==8 && (infieldIn||dpDepth)) { bases.Third = null; outs++; }
            else if (roll==1 && outfieldIn) { /* hold */ }
            else if (bothScore) { ScoreRun(); ScoreRun(); bases.Second = null; bases.Third = null; }
            else if (roll==5 && (infieldIn||dpDepth)) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==6 && !outfieldIn) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==8 && !(infieldIn||dpDepth)) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==9 && outfieldIn) { /* hold */ }
            else if (roll==10 && !outfieldIn) { bases.Third = bases.Second; bases.Second = null; }
            else if (roll==1 && !outfieldIn) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==3 && !outfieldIn) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==4 && !outfieldIn) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            else if (roll==7 && !(infieldIn||dpDepth)) { ScoreRun(); bases.Third = bases.Second; bases.Second = null; }
            bases.First = batter;
        }
        else if (on1 && on2 && on3)
        {
            playByPlayLog.Add($"[1B, BL] {NarrativeHelpers.GetSingleBasesLoadedNarrative(roll, safe, infieldIn, dpDepth, outfieldIn)}");
            if (roll==1 && outfieldIn) { /* hold */ }
            else if (roll==2 && (infieldIn||dpDepth)) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else if (roll==2) { ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else if (roll==3 && outfieldIn) { /* hold */ }
            else if (roll==3) { ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else if (roll==4 && outfieldIn) { /* hold */ }
            else if (roll==4) { ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else if (roll==5) { ScoreRun(); ScoreRun(); bases.Third = bases.First; bases.Second = null; bases.First = null; }
            else if (roll==6 && (infieldIn||dpDepth)) { outs++; }
            else if (roll==6) { ScoreRun(); outs++; }
            else if (roll==7) { ScoreRun(); }
            else if (roll==8 && outfieldIn) { /* hold */ }
            else if (roll==8) { ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            else if (roll==9) { ScoreRun(); ScoreRun(); bases.Third = bases.First; bases.Second = null; bases.First = null; }
            else if (roll==10) { ScoreRun(); bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            bases.First = batter;
        }
        else
        {
            // Fallback: advance each runner one base, batter to first
            ScoreAndClear(ref bases.Third);
            bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter;
        }
    }

    // --- Groundout Narratives & Advancement ---
    private void ResolveGroundoutWithRunners(string batterOption, string pitcherOption)
    {
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        bool infieldIn = string.Equals(pitcherOption, "Infield In", StringComparison.OrdinalIgnoreCase);
        bool dpDepth = string.Equals(pitcherOption, "Double Play Depth", StringComparison.OrdinalIgnoreCase);
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        int r = RollDie(10);
        // Bases empty
        if (!on1 && !on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetGroundoutBasesEmptyNarrative(r);
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            outs++;
            return;
        }

        // Runner on 1st only
        if (on1 && !on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetGroundoutR1Narrative(r, infieldIn, dpDepth, safe);
            switch (r)
            {
                case 1:
                    if (infieldIn) { bases.Second = bases.First; bases.First = null; outs++; }
                    else if (safe) { bases.Second = bases.First; bases.First = null; outs++; }
                    else { outs = Math.Min(3, outs + 2); bases.First = null; }
                    break;
                case 2:
                    if (infieldIn) { bases.Second = bases.First; bases.First = null; outs++; }
                    else if (safe) { bases.Second = bases.First; bases.First = null; outs++; }
                    else { outs = Math.Min(3, outs + 2); bases.First = null; }
                    break;
                case 3:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                case 4:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                case 5:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                case 6:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                case 7:
                    if (infieldIn) { bases.Second = bases.First; bases.First = null; outs++; }
                    else if (safe) { bases.Second = bases.First; bases.First = null; outs++; }
                    else { outs = Math.Min(3, outs + 2); bases.First = null; }
                    break;
                case 8:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                case 9:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
                default:
                    bases.Second = bases.First; bases.First = null; outs++;
                    break;
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Runner on 2nd only
        if (!on1 && on2 && !on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, drawn in — checks runner, throws to 1st; runner holds at 2nd." : "Grounder to SS, routine to 1st — runner holds at 2nd.",
                2 => infieldIn ? "Chopper to 2B, looks runner back, still goes to 1st — runner advances to 3rd late." : "Chopper to 2B, throw to 1st — runner advances to 3rd.",
                3 => "Slow roller to 3B, looks runner back, throw to 1st — runner holds.",
                4 => "Grounder up the middle, SS throws to 1st — runner takes 3rd.",
                5 => infieldIn ? "Hard grounder to 1B, checks runner then steps on bag — runner stays at 2nd." : "Hard grounder to 1B, unassisted — runner advances to 3rd.",
                6 => infieldIn ? "Bouncer off mound, pitcher checks runner and goes to 1st — runner retreats to 2nd." : "Bouncer off mound, pitcher to 1st — runner advances to 3rd without a play.",
                7 => infieldIn ? "Grounder toward 2B, looks runner back, goes to 1st — runner stays at 2nd." : "Grounder toward 2B, routine to 1st — runner moves to 3rd.",
                8 => infieldIn ? "Sharp grounder to 3B, checks runner then goes across — runner holds." : "Sharp grounder to 3B, across to 1st — runner holds at 2nd.",
                9 => infieldIn ? "Grounder through right side cut off; 1B checks runner then goes to 1st — runner to 3rd." : "Grounder through right side, toss to pitcher covering — runner takes 3rd.",
                10 => infieldIn ? "Soft roller to SS charging; looks home idea then to 1st — runner holds at 2nd." : "Soft roller to SS; just in time at 1st — runner advances to 3rd.",
                _ => "Routine grounder; batter out, runner to 3rd."
            };
            // Safe option nuances mentioned: advance only if ball clears pitcher cleanly
            if (safe && (r == 1 || r == 3 || r == 8))
            {
                // Stay at 2nd on plays in front
                narrative += " (Safe: runner holds).";
                outs++;
            }
            else
            {
                if (r is 1 or 3 or 8) { outs++; /* runner holds */ }
                else { bases.Third = bases.Second; bases.Second = null; outs++; }
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Runner on 3rd only
        if (!on1 && !on2 && on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, throw home — runner out at the plate." : "Grounder to SS, throw to 1st — runner scores easily.",
                2 => infieldIn ? "Chopper to 3B, fires home — close play, out!" : "Chopper to 3B, looks home then to 1st — runner breaks and scores.",
                3 => infieldIn ? "Grounder to 2B, throws home — runner tagged out." : "Grounder to 2B, throw to 1st — run scores.",
                4 => infieldIn ? "Soft roller to pitcher, fires home — out at plate." : "Soft roller to pitcher, throw to 1st — run scores.",
                5 => infieldIn ? "Hard grounder to 1B, comes home — bang-bang; could be out." : "Hard grounder to 1B, taken unassisted — runner scores.",
                6 => infieldIn ? "Bobbled at SS; throws home too late — run scores." : "Bouncer toward SS bobbled — only play at 1st, run in.",
                7 => infieldIn ? "Slow roller to 2B, quick throw home — close; usually out." : "Slow roller to 2B, flips to 1st — runner scores.",
                8 => infieldIn ? "Grounder to 3B, fired home — runner out." : "Grounder to 3B, goes to 1st — run scores.",
                9 => infieldIn ? "Chopper over pitcher; SS tries home — too late, run counts." : "Chopper over pitcher, SS to 1st — runner scores.",
                10 => infieldIn ? "Hard shot back to mound; pitcher throws home — possible tag out." : "Hard shot off mound; to 1st — runner scores easily.",
                _ => "Grounder; out at 1st, run scores."
            };
            // Safe option: runner holds at 3rd if throw goes home
            if (safe && (infieldIn && (r is 1 or 2 or 3 or 4 or 5 or 8 or 10)))
            {
                narrative += " (Safe: runner holds at 3rd)."; outs++;
            }
            else
            {
                if (infieldIn && (r is 1 or 2 or 3 or 4 or 8)) { bases.Third = null; outs++; /* out at home */ }
                else { ScoreAndClear(ref bases.Third); outs++; }
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 1st & 2nd
        if (on1 && on2 && !on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, throws to 3rd for force — lead runner out." : "Grounder to SS, 6–4–3 double play; R2 advances to 3rd.",
                2 => infieldIn ? "Chopper to 2B, throw to 3rd — gets lead runner." : "Chopper to 2B, relay late — runners advance.",
                3 => infieldIn ? "Slow roller to 3B; holds lead runner, goes to 1st — runners hold." : "Slow roller to 3B; sure out at 1st — both advance.",
                4 => infieldIn ? "Grounder up middle; SS to 3rd just late — runners safe." : "Grounder up the middle; steps on 2nd, throw to 1st — double play; R2 to 3rd.",
                5 => infieldIn ? "Bouncer to 1B; throw to 3rd for force — lead out." : "Bouncer to 1B; tries 3–6–3, late — runners at corners.",
                6 => infieldIn ? "Hard grounder to 3B; goes to 3rd for force — others safe." : "Hard grounder to 3B; 5–4–3 double play.",
                7 => infieldIn ? "Grounder to 2B; goes home/3rd to prevent advance — runners hold, batter out at 1st." : "Grounder to 2B; relay wide — no DP; runners move up.",
                8 => infieldIn ? "Sharp to SS; fires to 3rd for force — out, others safe." : "Sharp to SS; flips to 2B, strong to 1st — inning-ending DP.",
                9 => infieldIn ? "Bouncer to pitcher; goes to 3rd — lead safe on close play." : "Bouncer to pitcher; checks lead, throws to 1st — both advance.",
                10 => infieldIn ? "Through right side; 2B dives, throws to 3rd — safe all around." : "Through right side past dive — out at 1st, both runners advance.",
                _ => "Grounder; outs recorded with conventional advances."
            };
            // Apply base/outs approximations
            switch (r)
            {
                case 1: if (infieldIn) { outs++; bases.Second = null; } else { outs = Math.Min(3, outs + 2); bases.Second = null; bases.First = null; bases.Third = new Player(); }
                    break;
                case 2: if (infieldIn) { outs++; bases.Second = null; } else { outs++; bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    break;
                case 3: if (infieldIn) { outs++; } else { outs++; bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    break;
                case 4: if (infieldIn) { outs++; /* late at 3rd, safe all */ } else { outs = Math.Min(3, outs + 2); bases.Second = null; bases.First = null; bases.Third = new Player(); }
                    break;
                case 5: if (infieldIn) { outs++; bases.Second = null; } else { outs++; bases.Third = bases.Second; bases.Second = null; }
                    break;
                case 6: if (infieldIn) { outs++; bases.Second = null; } else { outs = Math.Min(3, outs + 2); bases.Second = null; bases.First = null; }
                    break;
                case 7: outs++; if (!infieldIn) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    break;
                case 8: if (infieldIn) { outs++; bases.Second = null; } else { outs = Math.Min(3, outs + 2); bases.Second = null; bases.First = null; }
                    break;
                case 9: outs++; if (!infieldIn) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    break;
                case 10: outs++; bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; break;
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 1st & 3rd
        if (on1 && !on2 && on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, throws home — runner tagged at plate." : "Grounder to SS, flips to 2nd, relay to 1st — double play; run scores.",
                2 => infieldIn ? "Slow roller to 2B, throws home — close play; runner may be out." : "Slow roller to 2B, only play at 1st — runner from 3rd scores.",
                3 => infieldIn ? "Chopper to 3B, fires home — runner out." : "Chopper to 3B, checks runner then 1st — run scores.",
                4 => infieldIn ? "Hard grounder to 2B, throws home — runner out." : "Hard grounder to 2B — turns quick double play; run scores.",
                5 => infieldIn ? "Grounder back to pitcher, throws home — out at plate." : "Grounder back to pitcher, throws to 1st — runner scores easily.",
                6 => infieldIn ? "Between 3B/SS; SS throws home — late, run scores." : "Between 3B/SS, deep — only play at 1st, run scores.",
                7 => infieldIn ? "Bouncer toward SS; looks home, runner retreats." : "Bouncer toward SS; feed to 2nd wide — one out, run scores.",
                8 => infieldIn ? "Grounder to 1B; throws home — runner out." : "Grounder to 1B; to 2nd, return late — run scores.",
                9 => "Sharp grounder to 3B, throw home — out at plate.",
                10 => infieldIn ? "Chopper off mound; SS goes home — close play, possible tag out." : "Chopper off mound; SS to 1st — run scores.",
                _ => "Grounder; out recorded, run decision per depth."
            };
            // Apply effects
            if (infieldIn && (r is 1 or 2 or 3 or 4 or 5 or 8 or 9 or 10))
            {
                bases.Third = null; outs++; // out at home
            }
            else
            {
                // run scores
                ScoreAndClear(ref bases.Third); outs++;
                // Handle possible DP ones (1 and 4 when not infield in)
                if (!infieldIn && (r == 1 || r == 4))
                {
                    outs = Math.Min(3, outs + 1); // add the second out
                    bases.First = null;
                }
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 2nd & 3rd
        if (!on1 && on2 && on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, throws home — runner out at plate; trail holds." : "Grounder to SS, throw to 1st — run scores; trail to 3rd.",
                2 => infieldIn ? "Chopper to 2B, throws home — close, could be out." : "Chopper to 2B, throw to 1st — run scores, trail up one.",
                3 => infieldIn ? "Slow roller to 3B, fires home — runner out." : "Slow roller to 3B, throw across — run scores.",
                4 => infieldIn ? "Grounder up the middle, SS throws home — late; run safe." : "Grounder up the middle; to 1st — run scores.",
                5 => infieldIn ? "Hard one-hopper to 1B, throws home — possible out." : "Hard one-hopper to 1B, unassisted — both advance, run scores.",
                6 => infieldIn ? "Grounder to SS, throw home — tag play; may be safe or out." : "Grounder to SS, runner breaks late — only play at 1st, run scores.",
                7 => infieldIn ? "Bouncer to 2B, comes home — runner out." : "Bouncer to 2B, routine to 1st — run scores.",
                8 => infieldIn ? "Grounder to 3B, goes home — runner out." : "Grounder to 3B, across to 1st — run scores.",
                9 => infieldIn ? "Slow roller up the line, pitcher checks then late home — runner safe." : "Slow roller up the line, flip to 1st — run scores, trail advances.",
                10 => infieldIn ? "Through right side; 2B from knees throws home — late, run scores." : "Through right side past dive — both advance safely, run scores.",
                _ => "Grounder; run likely scores with conventional advance."
            };
            if (infieldIn && (r is 1 or 2 or 3 or 7 or 8))
            {
                bases.Third = null; outs++; // out at home
            }
            else
            {
                ScoreAndClear(ref bases.Third); outs++;
                // trail to 3rd on most plays
                bases.Third = bases.Second; bases.Second = null;
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Bases loaded
        if (on1 && on2 && on3)
        {
            string narrative = r switch
            {
                1 => infieldIn ? "Grounder to SS, throws home for force; throw to 1st not in time." : "Grounder to SS, home for one, relay to 1st — double play; run prevented.",
                2 => infieldIn ? "Chopper to 3B, throws home for force; others advance." : "Chopper to 3B, steps on bag and fires to 1st — double play; run scores.",
                3 => infieldIn ? "Grounder to 2B, throws home for force; no other play." : "Grounder to 2B, over to short and back to first — double play; run scores.",
                4 => infieldIn ? "Bouncer to 1st, throws home immediately — out at plate." : "Bouncer to 1st, steps on bag, throw home — not in time, one run scores.",
                5 => infieldIn ? "Hard up the middle, SS throws home — out at plate." : "Hard up the middle, relay wide — only one out, run scores.",
                6 => infieldIn ? "Grounder to 3B, fires home — force at plate; relay to 1st late; bases stay loaded." : "Grounder to 3B, force at plate; relay late — bases loaded.",
                7 => infieldIn ? "Soft roller to SS, throws home; runner safe on bang-bang play." : "Soft roller to SS, to 2B for one; relay late — one run scores.",
                8 => infieldIn ? "Back to pitcher, home for force, perfect relay to 1st — double play." : "Back to pitcher, throws home for force, relay to first — double play; run erased.",
                9 => infieldIn ? "Bouncer to 2B, throws home — out at plate." : "Bouncer to 2B, looks home then to 1st — out; run scores.",
                10 => infieldIn ? "Through left side, SS dives, can’t get it — run scores." : "Through left side past diving SS — all advance safely, one run scores.",
                _ => "Grounder; plays at home dictate runs and outs."
            };
            // Apply state changes (approximate per narrative)
            switch (r)
            {
                case 1:
                    if (infieldIn) { outs++; bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    else { outs = Math.Min(3, outs + 2); /* force+1st */ bases.Third = bases.Second; bases.Second = null; bases.First = null; }
                    break;
                case 2:
                    if (infieldIn) { outs++; bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
                    else { outs = Math.Min(3, outs + 2); ScoreRun(); bases.Third = bases.Second; bases.Second = null; bases.First = null; }
                    break;
                case 3:
                    if (infieldIn) { outs++; /* force at home only */ }
                    else { outs = Math.Min(3, outs + 2); ScoreRun(); }
                    break;
                case 4:
                    if (infieldIn) { outs++; }
                    else { outs++; ScoreRun(); }
                    break;
                case 5:
                    if (infieldIn) { outs++; }
                    else { outs++; ScoreRun(); }
                    break;
                case 6:
                    outs++; // force at home
                    break;
                case 7:
                    if (infieldIn) { outs++; ScoreRun(); } else { outs += 2; ScoreRun(); }
                    break;
                case 8:
                    outs = Math.Min(3, outs + 2); // double play at plate-first
                    break;
                case 9:
                    if (infieldIn) { outs++; } else { outs++; ScoreRun(); }
                    break;
                case 10:
                    outs++; ScoreRun(); break;
            }
            playByPlayLog.Add($"[GROUNDOUT NARRATIVE] {narrative}");
            return;
        }

        // Fallback: conventional groundout one out, standard advances by force only
        playByPlayLog.Add("[GROUNDOUT NARRATIVE] Routine grounder handled; conventional outs and advances.");
        outs++;
    }

    // --- Flyout Narratives & Advancement ---
    private void ResolveFlyOutWithRunners(string batterOption, string pitcherOption, string flyType)
    {
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        bool outfieldIn = string.Equals(pitcherOption, "Outfield In", StringComparison.OrdinalIgnoreCase);
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        int r = RollDie(10);
        // If FO_SF was flagged, it was handled earlier. These are standard fly outs / popups.

        // Bases empty: descriptive only
        if (!on1 && !on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetFlyoutBasesEmptyNarrative(r);
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            outs++;
            return;
        }

        // Runner on 1st only
        if (on1 && !on2 && !on3)
        {
            string narrative;
            switch (r)
            {
                case 1:
                    narrative = outfieldIn ? "Fly to right near the line, OF in — no tag; runner holds." : "Fly to right near the line — runner bluffs but holds.";
                    outs++; break;
                case 2:
                    if (safe)
                    {
                        narrative = "Deep fly to center — Safe called, runner holds at first."; outs++; break;
                    }
                    narrative = outfieldIn ? "Deep fly to center, OF shallow — runner retreats; no advance." : "Deep fly to center, runner tags and advances to 2nd easily.";
                    outs++; if (!outfieldIn) { bases.Second = bases.First; bases.First = null; } break;
                case 3:
                    narrative = "High popup on the infield, shortstop calls it — runner stays."; outs++; break;
                case 4:
                    if (safe)
                    {
                        narrative = "Fly to left-center gap on the track — Safe: runner holds at 1st."; outs++; break;
                    }
                    narrative = outfieldIn ? "Fly to left-center, OF in — no tag." : "Fly to left-center gap on the track — runner tags to 2nd.";
                    outs++; if (!outfieldIn) { bases.Second = bases.First; bases.First = null; } break;
                case 5:
                    narrative = outfieldIn ? "Shallow fly to right, OF in — quick catch, runner retreats." : "Shallow fly to right, caught on the run — runner retreats quickly.";
                    outs++; break;
                case 6:
                    if (safe)
                    {
                        narrative = "Fly to deep right-center — Safe option keeps runner at 1st."; outs++; break;
                    }
                    narrative = outfieldIn ? "Fly to right-center, caught shallower — runner holds." : "Fly to deep right-center — tag up to 2nd on the throw.";
                    outs++; if (!outfieldIn) { bases.Second = bases.First; bases.First = null; } break;
                case 7:
                    narrative = "Fly to short left, LF camps — no tag attempt."; outs++; break;
                case 8:
                    narrative = "Line to center, caught; quick throw to 1st, close play — runner back in time."; outs++; break;
                case 9:
                    if (safe)
                    {
                        narrative = "Lofted fly to right-center — Safe: runner holds at 1st."; outs++; break;
                    }
                    narrative = "Lofted fly to right-center, caught; runner tags and makes it to 2nd on strong throw."; outs++; bases.Second = bases.First; bases.First = null; break;
                default:
                    narrative = "Fly to medium center — caught, runner holds."; outs++; break;
            }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Runner on 2nd only
        if (!on1 && on2 && !on3)
        {
            string narrative;
            bool advanceToThird = false;
            switch (r)
            {
                case 1:
                    // Tag unless Safe or Outfield In
                    advanceToThird = !(safe || outfieldIn);
                    narrative = advanceToThird ? "Fly ball to center, medium depth — runner tags and slides into 3rd." : outfieldIn ? "Fly to medium center, shallow positioning — runner holds at 2nd." : "Fly to medium center — runner holds (Safe).";
                    break;
                case 2:
                    advanceToThird = !outfieldIn; // Safe still allows tag (spec says same)
                    narrative = advanceToThird ? "Deep fly to right-center, warning track catch — runner tags to 3rd easily." : "Deep fly to right-center, OF shallow — runner forced to hold.";
                    break;
                case 3:
                    narrative = outfieldIn ? "Fly to shallow left, OF already in — runner anchored at 2nd." : "Fly to shallow left, charging catch — runner holds.";
                    break;
                case 4:
                    advanceToThird = !outfieldIn; // Safe same (deep automatic)
                    narrative = advanceToThird ? "Towering fly to right near wall — runner tags and advances to 3rd." : "Towering fly to right, shallow alignment — runner holds.";
                    break;
                case 5:
                    narrative = safe ? "High fly to straightaway center — Safe: runner stays at 2nd." : "High fly to straightaway center — runner bluffs, then retreats.";
                    break;
                case 6:
                    narrative = "Line drive to left, quick catch — runner dives back to 2nd to avoid double off.";
                    if (string.Equals(pitcherOption, "Double Play Depth", StringComparison.OrdinalIgnoreCase))
                        narrative += " (Double Play Depth pressure).";
                    break;
                case 7:
                    advanceToThird = !(safe || outfieldIn); // Safe same (guaranteed), Outfield In prevents
                    narrative = advanceToThird ? "Fly ball to deep left-center on track — runner tags to 3rd safely." : outfieldIn ? "Deep fly left-center, OF shallow — runner retreats." : "Deep fly left-center — Safe: runner holds.";
                    break;
                case 8:
                    narrative = outfieldIn ? "Soft fly to shallow right, OF charging — runner frozen at 2nd." : "Soft fly to shallow right, runner stays.";
                    break;
                case 9:
                    advanceToThird = !(safe || outfieldIn);
                    narrative = advanceToThird ? "Lofted fly to right field line — runner tags and advances to 3rd." : safe ? "Lofted fly to right field line — Safe: runner holds." : "Lofted fly to right field line, shallow alignment — runner holds.";
                    break;
                default: // 10
                    advanceToThird = !(safe || outfieldIn);
                    narrative = advanceToThird ? "Fly to deep center, runner tags and slides into 3rd just ahead of throw." : outfieldIn ? "Fly to deep center, CF shallow — runner retreats to 2nd." : "Fly to deep center — Safe: runner holds.";
                    break;
            }
            outs++;
            if (advanceToThird)
            {
                bases.Third = bases.Second; bases.Second = null;
            }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Runner on 3rd only
        if (!on1 && !on2 && on3)
        {
            string narrative; bool scores = false;
            switch (r)
            {
                case 1:
                    scores = !outfieldIn; // Safe still scores (automatic deep)
                    narrative = scores ? "Fly ball to deep center near wall — runner tags and scores easily." : "Fly to deep center, CF shallow — runner holds at 3rd.";
                    break;
                case 2:
                    scores = !(safe || outfieldIn); // Safe: hold; Outfield In: hold
                    narrative = scores ? "High fly to right, medium depth — runner tags and slides home safely." : safe ? "High fly to right — Safe: runner holds at 3rd." : "High fly to right, shallow alignment — runner holds.";
                    break;
                case 3:
                    narrative = "Fly to shallow left, runner bluffs but holds."; break;
                case 4:
                    scores = !outfieldIn; // Safe still scores
                    narrative = scores ? "Towering fly to left-center, warning track catch — runner scores standing." : "Towering fly left-center, OF shallow — runner stays.";
                    break;
                case 5:
                    scores = !(safe || outfieldIn);
                    narrative = scores ? "Fly to right field line, foul territory catch — runner tags and scores late." : safe ? "Fly to right field line — Safe: runner holds." : "Fly to right field line, shallow RF — runner retreats.";
                    break;
                case 6:
                    narrative = "Medium fly to center, shallow grass — runner holds."; break;
                case 7:
                    scores = !outfieldIn; // Safe still scores (deep)
                    narrative = scores ? "Deep drive to left, caught on track — runner tags and scores easily." : "Deep drive to left, OF shallow — runner holds.";
                    break;
                case 8:
                    narrative = "Fly to short right, caught on run — runner retreats immediately."; break;
                case 9:
                    scores = !outfieldIn; // Safe same
                    narrative = scores ? "Lofted fly to deep right-center — runner tags and slides home safely." : "Lofted fly right-center, shallow alignment — runner holds.";
                    break;
                default: // 10
                    narrative = "Fly to shallow center, diving grab — runner freezes, no advance."; break;
            }
            outs++;
            if (scores)
            {
                ScoreRun(); bases.Third = null;
            }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 1st & 2nd
        if (on1 && on2 && !on3)
        {
            string narrative; bool adv2To3 = false; bool adv1To2 = false;
            switch (r)
            {
                case 1:
                    adv2To3 = !(safe || outfieldIn); adv1To2 = adv2To3; // Safe: only lead advances, Outfield In: none
                    if (safe && !outfieldIn) adv1To2 = false;
                    narrative = adv2To3 ? (adv1To2 ? "Deep fly right-center — both runners tag and advance safely." : "Deep fly right-center — lead tags to 3rd; trail holds (Safe).") : outfieldIn ? "Deep fly right-center, OF shallow — both runners hold." : "Deep fly right-center — runners hold (Safe).";
                    break;
                case 2:
                    narrative = outfieldIn ? "Medium fly to left, shallow LF — both anchored." : "Medium fly to left — lead bluffs, all hold."; break;
                case 3:
                    adv2To3 = !(safe || outfieldIn); adv1To2 = adv2To3; if (safe && !outfieldIn) adv1To2 = false;
                    narrative = adv2To3 ? (adv1To2 ? "Deep fly to center, track catch — both runners advance." : "Deep fly to center — lead tags to 3rd; trail holds (Safe).") : outfieldIn ? "Deep fly to center, CF shallow — runners stay." : "Deep fly to center — Safe: runners hold.";
                    break;
                case 4:
                    adv2To3 = !(outfieldIn); // Safe same
                    narrative = adv2To3 ? "Fly to right medium-deep — lead tags to 3rd; trail holds." : "Fly to right medium-deep, shallow RF — no advance."; break;
                case 5:
                    narrative = "Line drive left-center on the run — runners retreat quickly."; break;
                case 6:
                    narrative = "Fly to shallow center charging catch — runners freeze."; break;
                case 7:
                    adv2To3 = !(safe || outfieldIn); adv1To2 = adv2To3; if (safe && !outfieldIn) adv1To2 = false;
                    narrative = adv2To3 ? (adv1To2 ? "Towering fly deep left, wall catch — both tag and advance." : "Towering fly deep left — lead advances; trail holds (Safe).") : outfieldIn ? "Towering fly deep left, OF shallow — no tag attempts." : "Towering fly deep left — Safe: hold both runners.";
                    break;
                case 8:
                    narrative = "Fly to short right charging RF — runners retreat."; break;
                case 9:
                    adv2To3 = !(safe || outfieldIn); adv1To2 = adv2To3; if (safe && !outfieldIn) adv1To2 = false;
                    narrative = adv2To3 ? (adv1To2 ? "Fly deep left-center track catch — both tag and advance." : "Fly deep left-center — lead tags; trail holds (Safe).") : outfieldIn ? "Fly deep left-center, OF shallow — both hold." : "Fly deep left-center — Safe: hold.";
                    break;
                default: // 10
                    narrative = outfieldIn ? "High fly medium left, shallow LF — no movement." : "High fly medium left — bluff, both stay."; break;
            }
            outs++;
            if (adv2To3)
            {
                bases.Third = bases.Second; bases.Second = null;
            }
            if (adv1To2)
            {
                bases.Second = bases.First; bases.First = null;
            }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 1st & 3rd
        if (on1 && !on2 && on3)
        {
            string narrative; bool score = false; bool adv1To2 = false;
            switch (r)
            {
                case 1:
                    score = !outfieldIn; adv1To2 = score && !safe; // Safe: trail holds
                    narrative = score ? (adv1To2 ? "Fly deep center near wall — run scores; runner from 1st tags to 2nd." : "Fly deep center near wall — run scores; trail holds (Safe).") : "Fly deep center, CF shallow — runner at 3rd holds.";
                    break;
                case 2:
                    narrative = outfieldIn ? "Medium fly to right, shallow RF — both hold." : "Medium fly to right — bluff and retreat."; break;
                case 3:
                    score = !outfieldIn; adv1To2 = score && !safe;
                    narrative = score ? (adv1To2 ? "Deep fly to left, track catch — run scores; runner from 1st advances." : "Deep fly to left — run scores; trail holds (Safe).") : "Deep fly to left, shallow alignment — run prevented.";
                    break;
                case 4:
                    narrative = "Shallow fly to center charging catch — both runners hold."; break;
                case 5:
                    score = !outfieldIn; adv1To2 = score && !safe;
                    narrative = score ? (adv1To2 ? "Fly to right-center on the move — run scores; trail to 2nd." : "Fly to right-center — run scores; trail holds (Safe).") : "Fly to right-center, shallow RF — runner on 3rd holds.";
                    break;
                case 6:
                    narrative = "Fly ball left-center medium depth — bluff and hold both."; break;
                case 7:
                    score = !outfieldIn; adv1To2 = score && !safe;
                    narrative = score ? (adv1To2 ? "Deep drive to right at wall — run scores; trail advances." : "Deep drive to right — run scores; trail holds (Safe).") : "Deep drive to right, OF shallow — run held at 3rd.";
                    break;
                case 8:
                    narrative = "Shallow left quick catch — runners stay."; break;
                case 9:
                    score = !outfieldIn; adv1To2 = score && !safe;
                    narrative = score ? (adv1To2 ? "Fly deep left-center — run scores easily; trail advances." : "Fly deep left-center — run scores; trail holds (Safe).") : "Fly deep left-center, shallow alignment — run prevented.";
                    break;
                default: //10
                    score = !outfieldIn; // trail forced to stay
                    narrative = score ? "Medium fly to right, strong throw but run scores standing; trail forced to stay." : "Medium fly to right, shallow RF — runner at 3rd holds.";
                    break;
            }
            outs++;
            if (score) { ScoreRun(); bases.Third = null; }
            if (adv1To2) { bases.Second = bases.First; bases.First = null; }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Runners on 2nd & 3rd
        if (!on1 && on2 && on3)
        {
            string narrative; bool score = false; bool adv2To3 = false;
            switch (r)
            {
                case 1:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Deep fly center track catch — run scores; runner from 2nd advances to 3rd." : "Deep fly center — run scores; runner from 2nd holds (Safe).") : "Deep fly center, CF shallow — both hold.";
                    break;
                case 2:
                    score = !(safe || outfieldIn);
                    narrative = score ? "Medium fly to right — runner tags and slides home safely; runner at 2nd holds." : safe ? "Medium fly to right — Safe: runner holds at 3rd." : "Medium fly to right, shallow RF — no tag attempt.";
                    break;
                case 3:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Fly left-center deep — both runners tag; run scores and trail to 3rd." : "Fly left-center deep — run scores; runner from 2nd holds (Safe).") : "Fly left-center deep, OF shallow — runners remain.";
                    break;
                case 4:
                    narrative = "Shallow fly to right charging catch — runners freeze."; break;
                case 5:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Deep left wall catch — run scores; runner from 2nd advances." : "Deep left wall catch — run scores; runner from 2nd holds (Safe).") : "Deep left, shallow alignment — both hold.";
                    break;
                case 6:
                    narrative = "Fly to straightaway center medium depth — bluff, runners hold."; break;
                case 7:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Towering fly right-center track catch — run scores; runner from 2nd to 3rd." : "Towering fly right-center — run scores; trail holds (Safe).") : "Towering fly right-center, shallow defenders — no tags.";
                    break;
                case 8:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Fly deep left fence — both tag; run scores; trail advances." : "Fly deep left fence — run scores; trail holds (Safe).") : "Fly deep left fence, shallow OF — holding both runners.";
                    break;
                case 9:
                    score = !(safe || outfieldIn);
                    narrative = score ? "Medium fly center — late tag, run scores; runner at 2nd holds." : safe ? "Medium fly center — Safe: runner holds at 3rd." : "Medium fly center shallow alignment — runners stay.";
                    break;
                default: //10
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Deep drive right field track catch — run scores; runner from 2nd advances." : "Deep drive right — run scores; runner from 2nd holds (Safe).") : "Deep drive right, shallow alignment — no advancement.";
                    break;
            }
            outs++;
            if (score) { ScoreRun(); bases.Third = null; }
            if (adv2To3) { bases.Third = bases.Second; bases.Second = null; }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Bases loaded
        if (on1 && on2 && on3)
        {
            string narrative; bool score = false; bool adv2To3 = false; bool adv1To2 = false;
            switch (r)
            {
                case 1:
                    score = !outfieldIn; adv2To3 = score && !safe; adv1To2 = adv2To3;
                    if (safe && !outfieldIn) adv1To2 = false;
                    narrative = score ? (adv2To3 ? (adv1To2 ? "Deep fly center near wall — run scores; other runners advance safely." : "Deep fly center — run scores; lead runner advances; trail holds (Safe).") : "Deep fly center — run scores; others hold.") : "Deep fly center, shallow CF — all runners hold.";
                    break;
                case 2:
                    score = !(safe || outfieldIn);
                    narrative = score ? "Fly to right medium depth — run tags and slides home; others stay." : safe ? "Fly to right medium depth — Safe: all hold." : "Fly to right medium depth, shallow RF — all hold.";
                    break;
                case 3:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Towering fly deep left-center — run scores; runner from 2nd tags to 3rd." : "Towering fly deep left-center — run scores; others hold (Safe).") : "Towering fly deep left-center, shallow OF — no advancement.";
                    break;
                case 4:
                    narrative = "Shallow fly to left charging catch — all runners freeze."; break;
                case 5:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Deep fly right-center wall — run scores; runner from 2nd to 3rd; runner from 1st holds." : "Deep fly right-center — run scores; others hold (Safe).") : "Deep fly right-center, shallow OF — lead runners hold.";
                    break;
                case 6:
                    narrative = "High fly straightaway center medium depth — bluff, all hold."; break;
                case 7:
                    score = !outfieldIn; adv2To3 = score && !safe; adv1To2 = adv2To3 && !safe; // Safe: no trailing advances
                    narrative = score ? (adv2To3 ? (adv1To2 ? "Fly left-center gap deep — run scores; other runners advance one base." : "Fly left-center gap deep — run scores; runner from 2nd advances; runner from 1st holds (Safe).") : "Fly left-center gap deep — run scores; others hold.") : "Fly left-center gap deep, shallow OF — none advance.";
                    break;
                case 8:
                    narrative = "Fly to shallow right charging RF — quick throw home; all runners hold."; break;
                case 9:
                    score = !outfieldIn; adv2To3 = score && !safe;
                    narrative = score ? (adv2To3 ? "Fly deep left just short of wall — run scores; runner from 2nd advances; runner from 1st holds." : "Fly deep left — run scores; others hold (Safe).") : "Fly deep left, shallow alignment — no advancement.";
                    break;
                default: //10
                    score = !outfieldIn;
                    narrative = score ? "Fly deep right track catch — run scores easily; others hold." : "Fly deep right, shallow OF — no tag attempts.";
                    break;
            }
            outs++;
            if (score) { ScoreRun(); bases.Third = null; }
            if (adv2To3) { bases.Third = bases.Second; bases.Second = null; }
            if (adv1To2) { bases.Second = bases.First; bases.First = null; }
            playByPlayLog.Add($"[FLYOUT NARRATIVE] {narrative}");
            return;
        }

        // Other base states: default flyout, runners hold (non-sac)
        playByPlayLog.Add("[FLYOUT NARRATIVE] Routine flyout; runners hold.");
        outs++;
    }

    // --- Strikeout Narratives & Runner Effects ---
    private void ResolveStrikeoutWithRunners(Player batter, Player pitcher, string batterOption, string pitcherOption)
    {
        // Determine base state
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        int r = RollDie(10);

        string narrative = "";
        void FinishStrikeoutDefault()
        {
            playByPlayLog.Add("[RESULT] Strikeout!");
            outs++;
            AddPitcherStrikeoutBonus(pitcher);
            AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            pitcher.StrikeoutBonus += 0.0025;
            if (outs == 3) EndHalfInning();
        }

        // Bases empty
        if (!on1 && !on2 && !on3)
        {
            narrative = r switch
            {
                1 => "Swing and a miss! Fastball blows by for strike three.",
                2 => "Caught looking! Painted the corner — strike three called.",
                3 => "Foul tip into the mitt — strike three.",
                4 => "Big breaker — batter waves over it, strike three.",
                5 => "Checked swing appeal… he went! Strike three!",
                6 => "Dropped third strike, catcher recovers and throws to first — batter out.",
                7 => "Dropped third strike squirts away — batter hustles and reaches!",
                8 => "High fastball chased — strike three swinging.",
                9 => "Devilish changeup on the inner edge — strike three called.",
                _ => "Late swing on the curve — strike three, side retired."
            };
            if (r == 7)
            {
                playByPlayLog.Add($"[K NARRATIVE] {narrative}");
                // Batter to first on dropped third strike
                if (bases.First == null) bases.First = batter; else if (bases.Second == null) { bases.Second = bases.First; bases.First = batter; } else { bases.Third = bases.Second; bases.Second = bases.First; bases.First = batter; }
                AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
                return;
            }
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            FinishStrikeoutDefault();
            return;
        }

        // Runner on 1st only
        if (on1 && !on2 && !on3)
        {
            narrative = r switch
            {
                1 => "Swing and a miss! Strike three — runner holds at 1st.",
                2 => "Strike three called — perfect pitch, no movement.",
                3 => "Dropped third strike! Catcher throws to 1st; runner from 1st advances.",
                4 => "High fastball chased — strike three swinging.",
                5 => "Breaking ball low — strike three, inning continues.",
                6 => "Block in dirt, flip to first — batter out; runner stays.",
                7 => "Dropped third strike — runner breaks! Throw to 2nd… out on the back end!",
                8 => "Called strike three; runner bluffs — no throw.",
                9 => "Swinging strikeout, runner takes off — safe at 2nd!",
                _ => "Missed bunt on 3rd strike — batter out, runner returns."
            };
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            // Apply effects
            if (r == 3)
            {
                // Batter out at first, R1 to second
                outs++; if (bases.Second == null) { bases.Second = bases.First; bases.First = null; }
            }
            else if (r == 7)
            {
                // Double play on throw to second: batter out at first, runner out at second
                int add = Math.Min(2, 3 - outs); outs += add; bases.First = null; bases.Second = null;
            }
            else if (r == 9)
            {
                // Runner steals 2nd on K
                outs++; if (bases.Second == null) { bases.Second = bases.First; bases.First = null; }
            }
            else
            {
                outs++;
            }
            AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            if (outs == 3) EndHalfInning();
            return;
        }

        // Runner on 2nd only
        if (!on1 && on2 && !on3)
        {
            narrative = r switch
            {
                1 => "Curveball outside corner, strike three looking — runner holds at 2nd.",
                2 => "Swing and miss, clean block — no advance.",
                3 => "Dropped third strike, out at first — runner advances to 3rd.",
                4 => "High fastball chased — strike three swinging.",
                5 => "Off-speed, frozen — strike three called.",
                6 => "In the dirt, blocked; batter out at 1st — runner stays.",
                7 => "Missed bunt, strike three — runner holds.",
                8 => "Strike three; runner bluffs for 3rd then stays.",
                9 => "Curveball in dirt, ball gets away — batter out at 1st, runner to 3rd.",
                _ => "Fastball on corner — strike three called."
            };
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            outs++;
            if (r == 3 || r == 9)
            {
                bases.Third = bases.Second; bases.Second = null;
            }
            AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            if (outs == 3) EndHalfInning();
            return;
        }

        // Runner on 3rd only
        if (!on1 && !on2 && on3)
        {
            narrative = r switch
            {
                1 => "Fastball high, swings through — strike three, runner stays.",
                2 => "Called strike three; runner bluffs and retreats.",
                3 => "Pitch in dirt, blocked — batter out at 1st, runner holds.",
                4 => "Swinging strikeout; dropped ball, throw to first beats batter; runner stays.",
                5 => "Breaking ball low — strike three.",
                6 => "Checked swing, called strike three — runner anchored.",
                7 => "Missed bunt, batter out — runner returns to 3rd.",
                8 => "Dropped third strike, catcher recovers — no advance by runner.",
                9 => "Called strike three on the inside edge — inning ends if third out.",
                _ => "Swing and miss, big fastball — strike three swinging."
            };
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            outs++;
            AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            if (outs == 3) EndHalfInning();
            return;
        }

        // Runners on 1st & 2nd
        if (on1 && on2 && !on3)
        {
            narrative = r switch
            {
                1 => "Swing and miss! Strike three — no advancement.",
                2 => "Called strike three; both runners hold.",
                3 => "Dropped third strike; throw to first — both runners advance safely.",
                4 => "High fastball chased — strike three swinging.",
                5 => "Curve low — strike three called; no play on runners.",
                6 => "Strike three, runner at 2nd breaks — throw down, out at 2nd!",
                7 => "Dropped third strike, batter out at first; runners hold.",
                8 => "Strike three swinging; runners go — double steal successful!",
                9 => "Breaking ball in dirt, caught — strike three.",
                _ => "Missed bunt attempt — strike three; runners retreat."
            };
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            // Apply effects
            if (r == 3)
            {
                outs++;
                // Advance both
                if (bases.Third == null) { bases.Third = bases.Second; bases.Second = bases.First; bases.First = null; }
            }
            else if (r == 6)
            {
                // K plus CS at 3rd? Spec says out at 2nd
                int add = Math.Min(2, 3 - outs); outs += add; bases.Second = null; // inning over if two outs made
            }
            else if (r == 8)
            {
                outs++; // K
                // Double steal
                bases.Third = bases.Second; bases.Second = bases.First; bases.First = null;
            }
            else
            {
                outs++;
            }
            AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            if (outs == 3) EndHalfInning();
            return;
        }

        // Bases loaded
        if (on1 && on2 && on3)
        {
            narrative = r switch
            {
                1 => "Swing and a miss! Strike three — threat ends.",
                2 => "Caught looking, perfect pitch freezes the batter.",
                3 => "Swinging strikeout, catcher drops it — recovers and steps on home for the force! Batter reaches, trail runners advance.",
                4 => "High fastball chased — strike three.",
                5 => "Curve catches the corner — strike three called.",
                6 => "Dropped third strike; runner from 3rd hesitates — catcher throws to first for the out.",
                7 => "Batter swings through — strike three, runners stranded.",
                8 => "Pitch in dirt, blocked perfectly — strikeout complete.",
                9 => "Checked swing, ruled he went — strike three.",
                _ => "Missed squeeze attempt — runner from 3rd retreats; batter out on strike three."
            };
            playByPlayLog.Add($"[K NARRATIVE] {narrative}");
            // Apply outs and base movement
            if (r == 3)
            {
                // Force out at home on runner from 3rd; batter reaches 1st; trail runners advance
                outs++;
                // Remove runner at 3rd (forced out)
                bases.Third = null;
                // Advance trail runners
                bases.Third = bases.Second; // runner from 2nd to 3rd
                bases.Second = bases.First; // runner from 1st to 2nd
                // Batter reaches first on dropped K
                bases.First = batter;
            }
            else
            {
                outs++;
            }
            AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
            if (outs == 3) EndHalfInning();
            return;
        }

        // Any other state (including R1&3, R2&3): default strikeout, runners hold
        playByPlayLog.Add("[K NARRATIVE] Strike three; runners hold.");
        outs++;
        AddPitcherStrikeoutBonus(pitcher); AddPitchCount(pitcher, OutcomePitchCost.Strikeout);
        if (outs == 3) EndHalfInning();
    }

    // --- Lineout Narratives & Double-Play Logic ---
    private void ResolveLineOutWithRunners(string batterOption, string pitcherOption)
    {
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        bool dpDepth = string.Equals(pitcherOption, "Double Play Depth", StringComparison.OrdinalIgnoreCase);
        bool outfieldIn = string.Equals(pitcherOption, "Outfield In", StringComparison.OrdinalIgnoreCase);
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        int r = RollDie(10);

        // Bases empty
        if (!on1 && !on2 && !on3)
        {
            string narrative = NarrativeHelpers.GetLineoutBasesEmptyNarrative(r);
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            return;
        }

        // Runner on 1st only
        if (on1 && !on2 && !on3)
        {
            string narrative; bool doublePlay = false;
            switch (r)
            {
                case 1:
                    doublePlay = dpDepth || !safe; // Safe prevents the throw-behind DP
                    narrative = doublePlay ? "Line to short, caught — throw to 1st, double play!" : "Line to short, caught — runner back just in time (Safe).";
                    break;
                case 2:
                    narrative = dpDepth ? "Liner to first, chest-high — nearly doubles off runner." : "Liner to first, chest-high — runner dives back safely.";
                    break;
                case 3:
                    narrative = dpDepth ? "Rocket to second, caught — throw behind narrowly misses DP." : "Rocket to second, caught — runner scrambles back.";
                    break;
                case 4:
                    narrative = outfieldIn ? "Line to left on the move — OF shallow; runner glued to the bag." : "Line to left on the move — runner retreats safely.";
                    break;
                case 5:
                    narrative = dpDepth ? "Hard liner toward short, snagged — close DP chance." : "Hard liner toward short, snagged — throw to 1st not in time.";
                    break;
                case 6:
                    narrative = "Line to right-center, caught on a slide — runner holds."; break;
                case 7:
                    doublePlay = true;
                    narrative = dpDepth ? "Sharp liner to first — 1B steps on bag for easy double play." : "Sharp liner to first, tag on bag — double play!";
                    break;
                case 8:
                    narrative = "Bullet to left-center, CF catches — runner retreats."; break;
                case 9:
                    doublePlay = dpDepth || !safe;
                    narrative = doublePlay ? "Line to short, caught — throws across, 6–3 double play!" : "Line to short, caught — runner back in time (Safe).";
                    break;
                default: // 10
                    doublePlay = dpDepth || !safe;
                    narrative = doublePlay ? "Rocket to second, throw behind runner — double play!" : "Rocket to second — quick throw behind, runner back safely (Safe).";
                    break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (doublePlay)
            {
                if (bases.First != null) bases.First = null;
                if (outs < 3) outs = Math.Min(3, outs + 1);
            }
            return;
        }

        // Runner on 2nd only
        if (!on1 && on2 && !on3)
        {
            string narrative; bool doublePlay = false;
            switch (r)
            {
                case 1:
                    doublePlay = dpDepth; // DP guaranteed on DP depth
                    narrative = doublePlay ? "Line to short, caught — throw to 2nd, double play!" : "Line to short, sharp catch — throw to 2nd just late.";
                    break;
                case 2:
                    narrative = "Hard liner to center, caught chest-high — runner dives back safely."; break;
                case 3:
                    narrative = dpDepth ? "Screaming liner to second — throw to 2nd nearly doubles runner." : "Screaming liner to second — throw just late."; break;
                case 4:
                    narrative = outfieldIn ? "Rocket to left, OF shallow — runner stays anchored." : "Rocket to left — runner bluffs and retreats."; break;
                case 5:
                    doublePlay = dpDepth; // DP on DP depth
                    narrative = doublePlay ? "Line up the middle, SS snags — flips to 2nd for double play!" : "Line up the middle, SS snags — runner barely back in time."; break;
                case 6:
                    narrative = "Bullet to right-center, caught on a dive — runner holds."; break;
                case 7:
                    narrative = dpDepth ? "Liner to third, throw to 2nd nearly doubles runner." : "Liner to third, quick reaction — throw to 2nd just late."; break;
                case 8:
                    narrative = "Line to left-center, caught on the run — runner returns safely."; break;
                case 9:
                    doublePlay = dpDepth; // DP on DP depth
                    narrative = doublePlay ? "Scorcher toward short, caught — fires to 2nd for double play!" : "Scorcher toward short, caught midair — runner slides back just in time."; break;
                default: // 10: per latest spec, DP unless Safe
                    doublePlay = dpDepth || !safe;
                    narrative = doublePlay ? "Liner to second, quick snag — runner from 2nd doubled off before getting back." : "Liner to second — Safe: runner returns safely.";
                    break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (doublePlay)
            {
                bases.Second = null; if (outs < 3) outs = Math.Min(3, outs + 1);
            }
            return;
        }

        // Runner on 3rd only
        if (!on1 && !on2 && on3)
        {
            string narrative; bool doublePlay = false;
            switch (r)
            {
                case 1:
                    narrative = dpDepth ? "Line to third, reflex grab — runner dives back, near double off." : "Line to third, reflex grab — runner dives back safely."; break;
                case 2:
                    narrative = "Liner to shortstop, chest-high — runner bluffs but holds."; break;
                case 3:
                    narrative = outfieldIn ? "Screaming liner to left, OF shallow — runner holds at 3rd." : "Screaming liner to left, caught on the move — runner holds at 3rd."; break;
                case 4:
                    narrative = "Line to right field, caught easily — runner bluffs and stays."; break;
                case 5:
                    narrative = dpDepth ? "Sharp liner to short, throw to 3rd — nearly doubles runner." : "Sharp liner to short, quick throw to 3rd — runner back in time."; break;
                case 6:
                    narrative = outfieldIn ? "Rocket to center, caught deep — outfield in prevents even a bluff." : "Rocket to center, caught deep — runner bluffs, retreats."; break;
                case 7:
                    narrative = "Liner to left-center, medium depth — runner freezes and stays."; break;
                case 8:
                    doublePlay = dpDepth || !safe;
                    narrative = doublePlay ? "Hard line to third, throw behind runner — double play!" : "Hard line to third — throw behind not in time (Safe)."; break;
                case 9:
                    narrative = outfieldIn ? "Line to left near track — outfield in prevents even the fake tag." : "Line to left, caught deep — runner tags but held by coach."; break;
                default:
                    narrative = "Rocket up the middle, SS leaping catch — runner bluffs, back safely."; break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (doublePlay)
            {
                bases.Third = null; if (outs < 3) outs = Math.Min(3, outs + 1);
            }
            return;
        }

        // Runners on 1st & 2nd
        if (on1 && on2 && !on3)
        {
            string narrative; int extraOuts = 0; bool triplePlay = false;
            switch (r)
            {
                case 1:
                    triplePlay = dpDepth || !safe; // automatic TP on hard contact at DP depth
                    narrative = triplePlay ? "Line to short, caught — to 2nd, across to 1st: triple play!" : "Line to short, caught — runners hold (Safe).";
                    break;
                case 2:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Rocket to third, reflex grab — throw to 2nd, double play!" : "Rocket to third, reflex grab — throw to 2nd just late.";
                    break;
                case 3:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Hard liner to second, caught — quick throw to 1st, double play!" : "Hard liner to second, throw to 1st just misses double off.";
                    break;
                case 4:
                    narrative = "Screaming liner to left on the move — both runners retreat safely."; break;
                case 5:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Line up the middle, SS behind the bag — flip to 2nd for double play!" : "Line up the middle, SS behind the bag — runner from 2nd back in time.";
                    break;
                case 6:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Bullet to first, caught and throw to 2nd — runner doubled off easily!" : "Bullet to first, caught — runner dives back to 2nd in time.";
                    break;
                case 7:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Liner to short on a leap — throw to 2nd completes the double play." : "Liner to short on a leap — throw to 2nd just misses.";
                    break;
                case 8:
                    narrative = "Line to right field, caught cleanly — runners retreat."; break;
                case 9:
                    extraOuts = dpDepth ? 2 : 1; // nearly triple on spec; DP depth makes it easy 4–6–3
                    narrative = extraOuts > 1 ? "Hard liner to second — 4–6–3 double play, nearly three outs!" : "Hard liner to second — fired to short for one, throw to 1st just late.";
                    break;
                default:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Line to third, caught sharply — throw across to 1st, double play!" : "Line to third, caught sharply — throw to 1st just late.";
                    break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (triplePlay)
            {
                bases.Second = null; bases.First = null; outs = Math.Min(3, outs + 2);
            }
            else if (extraOuts > 0)
            {
                // Prefer to double off the lead runner at 2nd, else trail at 1st
                if (bases.Second != null) { bases.Second = null; outs = Math.Min(3, outs + 1); }
                else if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
                if (extraOuts > 1 && outs < 3 && bases.First != null)
                {
                    bases.First = null; outs = Math.Min(3, outs + 1);
                }
            }
            return;
        }

        // Runners on 1st & 3rd
        if (on1 && !on2 && on3)
        {
            string narrative; int extraOuts = 0;
            switch (r)
            {
                case 1:
                    extraOuts = dpDepth ? 1 : 1; // default DP on 6–3 per spec
                    narrative = dpDepth ? "Line to short, caught — quick 6–3 double play; runner at 3rd holds." : "Line to short, caught — throw to 1st, double play; runner at 3rd holds.";
                    break;
                case 2:
                    extraOuts = dpDepth ? 1 : 1; // double play 5–3
                    narrative = "Rocket to third, caught on short hop — 3B fires to 1st, double play!"; break;
                case 3:
                    narrative = dpDepth ? "Hard liner to second, executed clean DP behind the runner." : "Hard liner to second, quick throw — nearly doubles runner off."; extraOuts = dpDepth ? 1 : 0; break;
                case 4:
                    narrative = "Line to right, caught easily — runner at 3rd bluffs but holds; R1 returns."; break;
                case 5:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Screaming liner up the middle, SS leaps, grabs — fires to 1st, double play!"; break;
                case 6:
                    narrative = "Bullet to left, caught in stride — both runners retreat."; break;
                case 7:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Line to first, snag and tag — double play!"; break;
                case 8:
                    narrative = "Liner to third on reflex — runner at 3rd dives back, no play."; break;
                case 9:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Sharp liner to short, quick throw to 1st — runner doubled off; inning over if two."; break;
                default:
                    narrative = "Line to center, caught — both runners bluff but stay."; break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (extraOuts > 0)
            {
                if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Runners on 2nd & 3rd
        if (!on1 && on2 && on3)
        {
            string narrative; int extraOuts = 0;
            switch (r)
            {
                case 1:
                    extraOuts = dpDepth ? 1 : 1; // 6–4 DP ends threat
                    narrative = "Line to short, caught — quick throw to 2nd, double play! Runner from 3rd back safely."; break;
                case 2:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Rocket to third, chest-level — throw to 2nd completes double play." : "Rocket to third — throw to 2nd just misses."; break;
                case 3:
                    extraOuts = dpDepth ? 1 : 1; // 2B to SS DP
                    narrative = "Liner to second, fired to short — trail runner doubled off!"; break;
                case 4:
                    narrative = "Line to left, caught easily — both runners hold."; break;
                case 5:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Sharp liner to short on a jump — runner from 2nd barely back; DP if in DP depth."; break;
                case 6:
                    narrative = "Hard liner to right, caught — runner from 3rd bluffs, runner from 2nd returns."; break;
                case 7:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Scorcher to third, throw to 2nd — gets both runners!" : "Scorcher to third — nearly doubles off trail runner."; break;
                case 8:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Bullet up the middle, SS dives and catches — quick throw to 2nd, double play!"; break;
                case 9:
                    narrative = "Line to center on the move — both runners retreat safely."; break;
                default:
                    narrative = dpDepth ? "Rocket to first base deep — nearly doubles lead runner at 3rd." : "Rocket to first base deep — fires to 3rd, runner back just in time."; break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (extraOuts > 0)
            {
                // Double off trail at 2nd by default
                if (bases.Second != null) { bases.Second = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Bases loaded
        if (on1 && on2 && on3)
        {
            string narrative; int extraOuts = 0; bool triplePlay = false;
            switch (r)
            {
                case 1:
                    triplePlay = dpDepth || !safe; // textbook 6–4–3
                    narrative = triplePlay ? "Line to short, flips to 2nd, relay to 1st — triple play!" : "Line to short, caught — everyone back safely (Safe).";
                    break;
                case 2:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Screaming liner to third — throw to 2nd doubles off runner!"; break;
                case 3:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Rocket to second, caught clean — throw to 1st, double play! Runner from 3rd back safely."; break;
                case 4:
                    narrative = "Liner to left, caught easily — all runners hold."; break;
                case 5:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Hard liner up the middle, SS snare — quick toss to 2nd, double play!"; break;
                case 6:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Line to third, reflex snag — throw across, double play at 1st!"; break;
                case 7:
                    extraOuts = dpDepth ? 1 : 0;
                    narrative = extraOuts > 0 ? "Liner to short on a leap — fires to 3rd, doubles off lead runner!" : "Liner to short on a leap — nearly doubles off runner at 3rd."; break;
                case 8:
                    narrative = "Bullet to right, caught on the run — runners retreat."; break;
                case 9:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Rocket to second, caught — flips to SS for double play; inning over if two already."; break;
                default:
                    extraOuts = dpDepth ? 1 : 1;
                    narrative = "Line to third, caught — quick step on bag, double play to end the frame!"; break;
            }
            playByPlayLog.Add($"[LINEOUT NARRATIVE] {narrative}");
            outs++;
            if (triplePlay)
            {
                // Remove R2 and R1 after batter out
                bases.Second = null; bases.First = null; outs = Math.Min(3, outs + 2);
            }
            else if (extraOuts > 0)
            {
                // Prefer forcing at 2nd or doubling trail
                if (bases.Second != null) { bases.Second = null; outs = Math.Min(3, outs + 1); }
                else if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Fallback: simple lineout, runners hold
        playByPlayLog.Add("[LINEOUT NARRATIVE] Line drive caught; runners hold.");
        outs++;
    }

    // --- Pop-Out Narratives & Runner Logic ---
    private void ResolvePopOutWithRunners(string batterOption, string pitcherOption, string popType)
    {
        // popType distinguishes infield variants (PO/PU) from others if needed later
        bool on1 = bases.First != null; bool on2 = bases.Second != null; bool on3 = bases.Third != null;
        bool infieldIn = string.Equals(pitcherOption, "Infield In", StringComparison.OrdinalIgnoreCase);
        bool dpDepth = string.Equals(pitcherOption, "Double Play Depth", StringComparison.OrdinalIgnoreCase);
        bool safe = string.Equals(batterOption, "Safe", StringComparison.OrdinalIgnoreCase);
        int r = RollDie(10);

        // Bases empty
        if (!on1 && !on2 && !on3)
        {
            string solo = NarrativeHelpers.GetPopoutBasesEmptyNarrative(r);
            playByPlayLog.Add($"[PO NARRATIVE] {solo}"); outs++; return;
        }

        // Runner on 1st only
        if (on1 && !on2 && !on3)
        {
            string narrative = r switch
            {
                1 => dpDepth && !safe ? "Pop to short — quick double off 1st!" : "Pop to short — runner scrambles back.",
                2 => "Sky-high near first — runner holds at the bag.",
                3 => dpDepth && !safe ? "Popup to second — throw behind nabs runner for DP." : "Popup to second — runner dives back safely.",
                4 => "Shallow pop behind first — RF drifts in, runner retreats.",
                5 => "High pop over the mound — runner remains.",
                6 => dpDepth && !safe ? "Soft pop to third — easy double off at first." : "Soft pop to third — runner back just in time.",
                7 => "Spinner toward catcher — runner frozen at first.",
                8 => "Pop to foul ground near first — caught, runner back.",
                9 => dpDepth && !safe ? "Popup to first — tag on bag doubles runner." : "Popup to first — runner retreats.",
                _ => "Infield pop — routine, runner holds."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}");
            outs++;
            // Apply double play cases
            if (!safe && dpDepth && (r == 1 || r == 3 || r == 6 || r == 9))
            {
                if (bases.First != null) bases.First = null;
                if (outs < 3) outs = Math.Min(3, outs + 1);
            }
            return;
        }

        // Runner on 2nd only
        if (!on1 && on2 && !on3)
        {
            string narrative = r switch
            {
                1 => dpDepth && !safe ? "Pop to short shallow — throw to 2nd doubles runner." : "Pop to short shallow — runner dives back.",
                2 => "High pop to third — runner holds at 2nd.",
                3 => "Popup to second — runner stays anchored.",
                4 => dpDepth && !safe ? "Soft pop to SS — quick relay doubles runner at 2nd." : "Soft pop to SS — runner back safely.",
                5 => "Spinner near mound — runner bluffs then returns.",
                6 => "Lofted pop to first side — runner remains at 2nd.",
                7 => dpDepth && !safe ? "Popup to 2B — snap throw behind gets runner for DP." : "Popup to 2B — runner alertly back.",
                8 => "Foul pop near 3B line — caught, runner holds.",
                9 => "High pop to second — routine, runner stationary.",
                _ => "Infield pop — runner holds at 2nd."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++;
            if (!safe && dpDepth && (r == 1 || r == 4 || r == 7))
            {
                if (bases.Second != null) bases.Second = null;
                if (outs < 3) outs = Math.Min(3, outs + 1);
            }
            return;
        }

        // Runner on 3rd only
        if (!on1 && !on2 && on3)
        {
            string narrative = r switch
            {
                1 => "Popup to short — runner at 3rd holds.",
                2 => "High pop to third — runner freezes.",
                3 => "Short popup to second — no tag attempt.",
                4 => "Foul pop near 3B box — caught, runner retreats.",
                5 => "Sky-high near mound — easy out; runner stays.",
                6 => "Pop behind 3rd — SS takes it; runner holds.",
                7 => "Lofted popup shallow left — runner bluffs then holds.",
                8 => "Soft pop to first side — caught, runner anchored.",
                9 => "High spinner above plate — catcher secures it; runner holds.",
                _ => "Routine infield pop — runner stays put."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++; return;
        }

        // Runners on 1st & 2nd
        if (on1 && on2 && !on3)
        {
            string narrative = r switch
            {
                1 => dpDepth && !safe ? "Pop to short — throws to 2nd, double play off lead runner." : "Pop to short — both runners back in time.",
                2 => dpDepth && !safe ? "Soft popup to 2B — snap throw to 1st completes double play." : "Soft popup to 2B — runners scramble back safely.",
                3 => "Sky-high near mound — both runners hold.",
                4 => dpDepth && !safe ? "Popup to third — quick relay to 2nd doubles runner." : "Popup to third — runners return.",
                5 => "Pop to first — runners retreat; no advance.",
                6 => "Shallow pop left side — SS takes it, runners anchored.",
                7 => dpDepth && !safe ? "Short pop to second — throw behind gets trailing runner too (near DP)." : "Short pop to second — trailing runner dives back.",
                8 => "Foul pop near 1B — caught; runners hold.",
                9 => "High infield pop — routine, no advancement.",
                _ => "Infield popup — station-to-station hold."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++;
            if (!safe && dpDepth && (r == 1 || r == 2 || r == 4 || r == 7))
            {
                // Prefer doubling lead at 2nd
                if (bases.Second != null) { bases.Second = null; outs = Math.Min(3, outs + 1); }
                else if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Runners on 1st & 3rd
        if (on1 && !on2 && on3)
        {
            string narrative = r switch
            {
                1 => dpDepth && !safe ? "Popup to short — throw to 1st doubles runner; runner at 3rd holds." : "Popup to short — runners dive back.",
                2 => "High pop to third — both hold.",
                3 => dpDepth && !safe ? "Soft pop to 2B — quick tag at 1st for DP." : "Soft pop to 2B — runners back safely.",
                4 => "Foul pop near 3B — caught, runners stay.",
                5 => "Sky-high near mound — no movement.",
                6 => dpDepth && !safe ? "Popup to 1B — step on bag doubles off runner." : "Popup to 1B — runner retreats in time.",
                7 => "Pop behind 3rd — SS catches; runners anchored.",
                8 => dpDepth && !safe ? "Popup to 3B — quick across to 1st for double play." : "Popup to 3B — throw across not in time to double.",
                9 => "Spinner above plate — catcher squeezes; runners hold.",
                _ => "Routine infield pop — both runners remain."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++;
            if (!safe && dpDepth && (r == 1 || r == 3 || r == 6 || r == 8))
            {
                // Double off runner at first
                if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Runners on 2nd & 3rd
        if (!on1 && on2 && on3)
        {
            string narrative = r switch
            {
                1 => "Popup to short — runners at 2nd/3rd hold.",
                2 => "High pop to third — both runners retreat.",
                3 => "Soft pop to second — no tag attempts.",
                4 => "Foul pop near 3B — caught; runners hold.",
                5 => "Sky-high over mound — routine out; runners stay.",
                6 => "Popup shallow left — SS takes it, no advance.",
                7 => "Pop toward first side — runners freeze.",
                8 => "Spinner near plate — catcher secures; runners back.",
                9 => "Lofted pop near second — easy catch; runners remain.",
                _ => "Infield pop — conservative hold at 2nd and 3rd."
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++; return;
        }

        // Bases loaded
        if (on1 && on2 && on3)
        {
            string narrative = r switch
            {
                1 => dpDepth && !safe ? "Popup to short — quick flip to 2nd then relay nearly nabs 1st (double play)." : "Popup to short — all runners scramble back.",
                2 => dpDepth && !safe ? "Soft pop to 2B — snap throw to 1st doubles runner." : "Soft pop to 2B — everyone retreats safely.",
                3 => "Sky-high above plate — catcher takes it; runners remain.",
                4 => "Foul pop near 1B — caught, bases remain loaded.",
                5 => "Pop on left side — SS gloves; no advance.",
                6 => dpDepth && !safe ? "Popup to 3B — throw to 1st doubles trailing runner." : "Popup to 3B — throw across not in time for DP.",
                7 => "High spinner near mound — pitcher records the out; all hold.",
                8 => "Short pop behind 2B — 2B makes catch; runners anchored.",
                9 => dpDepth && !safe ? "Popup to first — step on bag for quick double play; runners return." : "Popup to first — runner back in time.",
                _ => "Routine infield pop — station-to-station hold." 
            };
            playByPlayLog.Add($"[PO NARRATIVE] {narrative}"); outs++;
            if (!safe && dpDepth && (r == 1 || r == 2 || r == 6 || r == 9))
            {
                // Double off trail at first by default
                if (bases.First != null) { bases.First = null; outs = Math.Min(3, outs + 1); }
            }
            return;
        }

        // Fallback
        playByPlayLog.Add("[PO NARRATIVE] Infield popup; runners hold."); outs++;
    }

    private void EndHalfInning()
    {
        outs = 0;
        if (currentTeamBatting == awayTeamName)
        {
            currentTeamBatting = homeTeamName; // Go to bottom of inning (Home team bats)
        }
        else
        {
            currentTeamBatting = awayTeamName; // Go to next full inning (Away team bats)
            inning++;
        }
        // --- FIX IMPLEMENTED HERE: Skip pause if Simulation Only ---
        if (controlMode != "Simulation Only")
        {
            Console.Write("Press any key to start next half-inning...");
            Console.ReadKey(true);
        }
        // If it is Simulation Only, the loop runs immediately to the next half-inning.
    }

    private bool CheckMercyRule()
    {
        // Mercy Rule: 10+ runs after 5 full innings.
        if (inning >= 5 && Math.Abs(scoreHome - scoreAway) >= 10)
        {
            Console.WriteLine("\n*** MERCY RULE ACTIVATED! ***");
            return true;
        }
        return false;
    }

    private void DisplayGameStatus()
    {
        // Displays the game status using the color-coding you requested
        Console.Clear();
        
        Console.WriteLine("==================================================================");
        Console.WriteLine($" GAME IN PROGRESS at {location}");
        Console.WriteLine("==================================================================");
        
        // Scoreboard Display
        Console.WriteLine($" {awayTeamName} (Away): {scoreAway}  |  {homeTeamName} (Home): {scoreHome}");
        Console.WriteLine($" INNING: {GetInningLabel()} | OUTS: {outs}");
        
        // Dynamic Team Color (Simulated in Console)
        Console.ResetColor(); // Reset before applying new colors
        if (currentTeamBatting == awayTeamName)
        {
            // Simulate away team colors (e.g., White text on Black background)
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            // Simulate home team colors (e.g., Yellow text on Blue background)
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        
        // Display control type in the current action area
        string actionPrompt = controlMode == "Simulation Only" ? "RUNNING SIMULATION" : "Press 'M' for Options";
        
        Console.WriteLine($"\n CURRENT BATTER: {currentTeamBatting} ({actionPrompt})");

        Console.ResetColor();
        
        // Placeholder for Options Menus (L, U, B, X)
        Console.WriteLine("\n [1] Offensive Options (Batter) | [2] Defensive Options (Pitcher)");
        Console.WriteLine(" [S] Substitution | [V] View Box Score | [Q] Quit/Save Game");
        Console.WriteLine("==================================================================");
        Console.WriteLine(" PLAY-BY-PLAY LOG:");
        Console.WriteLine("------------------------------------------------------------------");
        // Show last 10 events (scrolling)
        int startIdx = Math.Max(0, playByPlayLog.Count - 10);
        for (int i = startIdx; i < playByPlayLog.Count; i++)
        {
            Console.WriteLine(playByPlayLog[i]);
        }
        Console.WriteLine("------------------------------------------------------------------");
    }

    private void DisplayPlayByPlayLog()
    {
        // Already shown in DisplayGameStatus, but can be used for other screens if needed
    }

    private void ViewBoxScore()
    {
        Console.Clear();
        Console.WriteLine("================ BOX SCORE ================");
        Console.WriteLine($"{awayTeamName}: {scoreAway}");
        Console.WriteLine($"{homeTeamName}: {scoreHome}");
        Console.WriteLine("===========================================");
        Console.WriteLine("Press any key to return to game...");
        Console.ReadKey(true);
    }

    private void SaveAndQuit()
    {
        // TODO: Implement actual save logic if needed
        Console.WriteLine("Saving game...");
        // Simulate save
        Thread.Sleep(500);
        Console.WriteLine("Game saved. Returning to main menu.");
        Thread.Sleep(500);
        _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
    }
    
    private string GetInningLabel()
    {
        // T for Top (Away team batting), B for Bottom (Home team batting)
        return $"{inning}{(currentTeamBatting == awayTeamName ? 'T' : 'B')}";
    }

    private void EndGameScreen()
    {
        Console.Clear();
        Console.WriteLine("\n==================================================");
        Console.WriteLine("                 F I N A L  S C O R E");
        Console.WriteLine("==================================================");
        Console.WriteLine($" {awayTeamName}: {scoreAway}");
        Console.WriteLine($" {homeTeamName}: {scoreHome}");
        Console.WriteLine("==================================================");
        Console.WriteLine("Press any key to return to the Main Menu...");
        Console.ReadKey(true);
        _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
    }
}
}
