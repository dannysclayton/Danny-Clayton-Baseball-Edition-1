using System;
using System.Threading;
using System.Linq;

public class GameLoopScreen
{
    private readonly GameEngine _engine;
    
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

    public GameLoopScreen(GameEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Initializes and starts the exhibition game loop.
    /// </summary>
    public void StartExhibitionGame(string teamA, string teamB, string gameLocation, string controlType)
    {
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
            // Display the current state and prompt for action
            DisplayGameStatus();
            
            // In a real app, logic would branch here based on controlType
            
            Console.WriteLine($"\n[ACTION] {currentTeamBatting} is deciding their offensive strategy...");
            
            // Simulate At-Bat actions
            if (controlMode != "Simulation Only") 
            {
                // Pause for dramatic effect in observer/user modes
                Thread.Sleep(1500); 
            }

            SimulateAtBat();

            if (CheckMercyRule()) break;
        }
        
        // Game Over screen placeholder
        EndGameScreen();
    }
    
    private void SimulateAtBat()
    {
        // This is where the d6 rolls, chart lookups, and modifier calculations happen.
        
        // Simple placeholder to change scores and simulate movement
        if (new Random().Next(1, 10) == 1)
        {
            Console.WriteLine("\n[RESULT] HOME RUN! (Simulated)");
            if (currentTeamBatting == homeTeamName) scoreHome++; else scoreAway++;
            outs = 0; // HR resets outs count in the middle of an at-bat, but keeps the flow simple
        }
        else
        {
            Console.WriteLine("\n[RESULT] Strikeout! (Simulated)");
            outs++;
        }
        
        if (outs == 3)
        {
            EndHalfInning();
        }
    }

    private void EndHalfInning()
    {
        Console.WriteLine($"\n--- End of {GetInningLabel()} ---");
        outs = 0;
        
        // Switch team batting
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
