using System;
using System.Linq;

public class ExhibitionGameScreen
{
    private readonly GameEngine _engine;
    private readonly GameLoopScreen _gameLoop; // Requires GameLoopScreen.cs
    
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

    public ExhibitionGameScreen(GameEngine engine)
    {
        _engine = engine;
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
        Console.WriteLine("\n--- PICK TEAMS ---");
        Console.WriteLine("Note: Full team selection logic (Conference->Region->Team) is a future step.");

        Console.Write("Enter Team A Name: ");
        teamA = Console.ReadLine()?.Trim() ?? "Away Team";

        Console.Write("Enter Team B Name: ");
        teamB = Console.ReadLine()?.Trim() ?? "Home Team";

        Console.WriteLine($"Matchup set: {teamA} vs {teamB}.");
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
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
