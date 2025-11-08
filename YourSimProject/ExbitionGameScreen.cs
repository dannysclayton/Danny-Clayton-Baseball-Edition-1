using YourSimProject.Services;
using YourSimProject.Models;
using System;
using System.Linq;

namespace YourSimProject
{
    public class ExhibitionGameScreen
    {
        private readonly GameEngine _engine;
        private readonly GameLoopScreen _gameLoop; // Requires GameLoopScreen.cs
        private readonly TeamDatabase _teamDatabase;

        // Exhibition Game Options
        private const string PICK_TEAMS = "Pick Teams";
        private const string SET_CONDITIONS = "Set Game Conditions";
        private const string CHOOSE_CONTROL = "Choose Control Type";
        private const string START_GAME = "Start Game";

        // Temporary Storage for Exhibition Settings
        private string teamA = "Not Set";
        private string teamB = "Not Set";
        private string location = "Default";
        private string controlType = "Not Set";

        public ExhibitionGameScreen(GameEngine engine, TeamDatabase teamDatabase)
        {
            _engine = engine;
            _teamDatabase = teamDatabase;
            // NOTE: GameLoopScreen must be instantiated here before use
            _gameLoop = new GameLoopScreen(engine);
        }

        public void DisplayAndHandle()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- 3RD BASE: EXHIBITION GAME SETUP ---");
                Console.WriteLine($" [1] {PICK_TEAMS} (A: {teamA}, B: {teamB})");
                Console.WriteLine($" [2] {SET_CONDITIONS} (Location: {location})");
                Console.WriteLine($" [3] {CHOOSE_CONTROL} (Type: {controlType})");
                Console.WriteLine($" [S] {START_GAME}");
                Console.WriteLine($" [B]ack to Main Menu");

                Console.Write("Enter selection: ");
                string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
                _engine.PlayClickSound();

                switch (input)
                {
                    case "1":
                        HandlePickTeams();
                        break;
                    case "2":
                        HandleSetGameConditions();
                        break;
                    case "3":
                        HandleChooseControlType();
                        break;
                    case "S":
                        if (HandleStartGame())
                        {
                            return; // Exits the Exhibition screen loop to start the game
                        }
                        break;
                    case "B":
                        _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
                        return;
                    default:
                        Console.WriteLine("\nInvalid selection. Press any key to continue.");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private void HandlePickTeams()
        {
            // Suppress nullability warnings for _engine.Teams in this routine after explicit runtime checks
            #pragma warning disable CS8602
        // Always refresh the team list from the database before selection
        if (_engine != null)
        {
            _engine.Teams = _teamDatabase.Conferences
                .SelectMany(c => c.Regions)
                .SelectMany(r => r.Teams)
                .ToList();
        }
        Console.WriteLine("\n--- PICK TEAMS ---");
        if (_engine.Teams == null || _engine.Teams.Count == 0)
        {
            Console.WriteLine("[ERROR] No teams loaded in engine context. Load or create teams first.");
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
        var teams = _engine.Teams!; // Null-checked above
        int teamCount = teams.Count;
    Console.WriteLine($"[DIAGNOSTIC] Number of teams loaded: {teamCount}");
        if (teamCount > 0)
        {
            Console.WriteLine("[DIAGNOSTIC] Team names:");
            if (teams != null)
            foreach (var t in teams)
            {
                Console.WriteLine($" - {t.Name}");
            }
        }
        Console.WriteLine("Select teams for the exhibition game:");

        if (teamCount < 2)
        {
            Console.WriteLine("[ERROR] Not enough teams loaded. Please upload or create teams first.");
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // Safe enumeration: teams is guaranteed non-null above
        for (int i = 0; i < teamCount; i++)
        {
            Console.WriteLine($" [{i + 1}] {teams[i].Name}");
        }

        Console.Write("Select Team A by number: ");
        int teamAIdx = -1;
    while (!int.TryParse(Console.ReadLine(), out teamAIdx) || teamAIdx < 1 || teamAIdx > teamCount)
        {
            Console.Write("Invalid selection. Enter Team A number: ");
        }
    teamA = teams[teamAIdx - 1].Name;

        Console.Write("Select Team B by number: ");
        int teamBIdx = -1;
        while (!int.TryParse(Console.ReadLine(), out teamBIdx) || teamBIdx < 1 || teamBIdx > teamCount || teamBIdx == teamAIdx)
        {
            Console.Write("Invalid selection. Enter Team B number (must be different from Team A): ");
        }
    teamB = teams[teamBIdx - 1].Name;

    Console.WriteLine($"Matchup set: {teamA} vs {teamB}.");
    Console.Write("Press any key to continue...");
    Console.ReadKey(true);
    #pragma warning restore CS8602
    }

    private void HandleSetGameConditions()
    {
        Console.WriteLine("\n--- SET GAME CONDITIONS ---");
        Console.WriteLine("Note: All teams use the Designated Hitter rule by default.");

        Console.Write("Enter Location (e.g., Stadium Name): ");
        location = Console.ReadLine()?.Trim() ?? "Custom Field";
        
        Console.WriteLine($"Conditions set: Location = {location}.");
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    private void HandleChooseControlType()
    {
        Console.WriteLine("\n--- CHOOSE CONTROL TYPE ---");
        Console.WriteLine(" [1] User vs Computer");
        Console.WriteLine(" [2] User vs User");
        Console.WriteLine(" [3] Computer vs Computer");
        Console.WriteLine(" [4] Simulation Only");

        Console.Write("Select Control Type: ");
        string input = Console.ReadKey(true).KeyChar.ToString();
        
        controlType = input switch
        {
            "1" => "User vs Computer",
            "2" => "User vs User",
            "3" => "Computer vs Computer",
            "4" => "Simulation Only",
            _ => "Not Set"
        };
        Console.WriteLine($"\nControl type set to: {controlType}.");
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    private bool HandleStartGame()
    {
        if (teamA == "Not Set" || teamB == "Not Set")
        {
            Console.WriteLine("\nError: Please set both Team A and Team B (Option 1). Press any key to continue.");
            Console.ReadKey(true);
            return false;
        }
        
        if (controlType == "Not Set")
        {
            Console.WriteLine("\nError: Please choose a Control Type (Option 3). Press any key to continue.");
            Console.ReadKey(true);
            return false;
        }

        // --- LAUNCH THE GAME LOOP ---
        _gameLoop.StartExhibitionGame(teamA, teamB, location, controlType);
        
        // Execution transfers to GameLoopScreen.cs until the game ends.
    return true;
}
    }
}
