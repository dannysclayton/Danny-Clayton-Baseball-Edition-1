using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using YourSimProject.Services;
using YourSimProject;

// NOTE: This class assumes the existence of the SeasonDataService, SeasonState, GameEngine, and other models.
public class SeasonScreen
{
    private readonly GameEngine _engine;
    private readonly SeasonDataService _dataService = new(); 
    private SeasonState _currentSeasonState = new(); 
    
    // Temporary Storage for New Season Settings
    private Dictionary<string, string> tempSeasonSettings = new();
    
    public SeasonScreen(GameEngine engine)
    {
        _engine = engine;
    }

    public void DisplayAndHandle()
    {
        Console.Clear();
        Console.WriteLine("\n--- 1ST BASE: SEASON MANAGEMENT ---");
        Console.WriteLine($" [C]reate Season (Current Status: {GetCreationStatus()})");
        Console.WriteLine($" [L]oad Season");
        Console.WriteLine($" [D]elete Season");
        Console.WriteLine($" [B]ack to Main Menu");
        
        Console.Write("Select an option: ");
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        _engine.PlayClickSound();
        Console.WriteLine();

        switch (input)
        {
            case "C":
                HandleCreateSeasonFlow();
                break;
            case "L":
                HandleLoadSeason();
                break;
            case "D":
                HandleDeleteSeason();
                break;
            case "B":
                _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
                break;
            default:
                Console.WriteLine("Invalid selection.");
                break;
        }
    }

    private string GetCreationStatus()
    {
        if (tempSeasonSettings.Count == 0) return "New";
        
        bool isComplete = 
            tempSeasonSettings.ContainsKey("SeasonLength") && tempSeasonSettings.ContainsKey("TeamStructure") && 
            tempSeasonSettings.ContainsKey("TeamControl") && tempSeasonSettings.ContainsKey("Playoffs");
        
        return isComplete ? "Ready to Save" : "In Progress";
    }

    #region Create Season Flow
    
    private void HandleCreateSeasonFlow()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- CREATE SEASON: DEFINE SETTINGS ---");
            Console.WriteLine("Select a step to define the season (Selections are temporary until saved):");
            
            // Display current status of each step
            Console.WriteLine($" [1] Pick Season Length (Current: {tempSeasonSettings.GetValueOrDefault("SeasonLength", "Not Set")})");
            Console.WriteLine($" [2] Pick Conferences, Regions, Teams (Current: {tempSeasonSettings.GetValueOrDefault("TeamStructure", "Not Set")})");
            Console.WriteLine($" [3] Team Control (Current: {tempSeasonSettings.GetValueOrDefault("TeamControl", "Not Set")})");
            Console.WriteLine($" [4] Play-Off Settings (Current: {tempSeasonSettings.GetValueOrDefault("Playoffs", "Not Set")})");
            Console.WriteLine($" [S] Save Season");
            Console.WriteLine($" [B]ack to Season Management");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();
            Console.WriteLine();

            switch (input)
            {
                case "1": PickSeasonLength(); break;
                case "2": PickTeamStructure(); break;
                case "3": PickTeamControl(); break;
                case "4": PickPlayoffSettings(); break;
                case "S": SaveSeason(); return; 
                case "B": return; 
                default: Console.WriteLine("Invalid selection. Press any key to try again."); 
                         Console.ReadKey(true); 
                         break;
            }
        }
    }

    private void PickSeasonLength()
    {
        Console.Write("\nLength of series vs other region's teams (1=1 Game, 2=3 Games, 3=5 Games): ");
    string input1 = Console.ReadLine() ?? "";
        
        Console.Write("Number of series vs opponents in the same region (1=2 Series, 2=3 Series, 3=5 Series): ");
    string input2 = Console.ReadLine() ?? "";
        
        Console.Write("Number of games in each series vs same region opponents (1=3 Games, 2=5 Games, 3=6 Games): ");
    string input3 = Console.ReadLine() ?? "";
        
        tempSeasonSettings["SeasonLength"] = $"Series:{input1} | RegionalSeries:{input2} | RegionalGames:{input3}";
        
        Console.Write("\nReady to Pick Conferences, Regions, Teams? (Y/N): ");
        if (Console.ReadKey(true).KeyChar.ToString().ToUpper() == "Y")
        {
             // Automatic progression logic is now removed to maintain menu control
        }
    }

    private void PickTeamStructure()
    {
        Console.WriteLine("\n--- PICK TEAMS ---");
        Console.WriteLine($" [U]se Loaded Conferences, Regions, Teams (Recommended)");
        Console.WriteLine($" [C]ustomize Conferences, Regions, Teams (Drag/Drop Interface)");
        
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        if (input == "U")
        {
            tempSeasonSettings["TeamStructure"] = "Loaded";
        }
        else if (input == "C")
        {
            tempSeasonSettings["TeamStructure"] = "Customized";
        }
    }

    private void PickTeamControl()
    {
        Console.WriteLine("\n--- TEAM CONTROL ---");
        Console.WriteLine(" 1. All Teams user control"); 
        Console.WriteLine(" 2. One team user control"); 
        Console.WriteLine(" 3. Multiple team user control"); 
        Console.WriteLine(" 4. Computer vs Computer team control"); 
        Console.WriteLine(" 5. Simulation"); 

        Console.Write("Select Control Type: ");
    string input = Console.ReadLine() ?? "";
    tempSeasonSettings["TeamControl"] = input;
    }

    private void PickPlayoffSettings()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- 4. PLAY-OFF SETTINGS ---");
            Console.WriteLine("Select the base playoff structure:");
            
            Console.WriteLine(" [H]igh School (Regionals/Wildcards)");
            Console.WriteLine(" [C]ollege (64-Team Tournament)");
            Console.WriteLine(" [B]ack to Create Season Menu");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();
            Console.WriteLine();

            switch (input)
            {
                case "H":
                    HandleHighSchoolPlayoffs();
                    DefineSeriesFormat();
                    tempSeasonSettings["Playoffs"] = "High School - Set";
                    return;
                case "C":
                    HandleCollegePlayoffs();
                    DefineSeriesFormat();
                    tempSeasonSettings["Playoffs"] = "College - Set";
                    return;
                case "B":
                    return;
                default:
                    Console.WriteLine("Invalid selection. Press any key to try again.");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    private void HandleHighSchoolPlayoffs()
    {
        Console.WriteLine("\n--- HIGH SCHOOL PLAYOFFS: SET QUALIFIERS ---");
        Console.Write("Use Wildcards? (Y/N): ");
        string wildcardInput = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        Console.WriteLine(wildcardInput); 

        if (wildcardInput == "Y")
        {
            Console.Write("Enter number of desired wildcard positions: ");
            if (int.TryParse(Console.ReadLine(), out int numWildcards) && numWildcards >= 0)
            {
                tempSeasonSettings["HS_Qualifiers"] = $"Wildcards: {numWildcards}";
                Console.WriteLine("Wildcard setup saved.");
            }
            else
            {
                tempSeasonSettings["HS_Qualifiers"] = "Wildcards: 0 (Invalid input)";
                Console.WriteLine("Invalid input. Using 0 wildcards.");
            }
        }
        else
        {
            tempSeasonSettings["HS_Qualifiers"] = "No Wildcards (Region Champions only)";
            Console.WriteLine("Using Region Champions and Runner-ups only.");
        }
    }

    private void HandleCollegePlayoffs()
    {
        Console.WriteLine("\n--- COLLEGE PLAYOFFS: 64 TEAM FIELD ---");
        Console.WriteLine(" [1] Manually assign teams to 64 team field");
        Console.WriteLine(" [2] Load top 64 teams to tournament (Auto-seed)");

        Console.Write("Enter selection: ");
        string input = Console.ReadKey(true).KeyChar.ToString();
        Console.WriteLine(input); 

        if (input == "1")
        {
            tempSeasonSettings["College_Field"] = "Manual Assignment";
            Console.WriteLine("Manual assignment interface loaded (Placeholder for list/table).");
        }
        else if (input == "2")
        {
            tempSeasonSettings["College_Field"] = "Load Top 64 Teams (Auto-seed)";
            Console.WriteLine("Auto-seeding logic selected.");
        }
        else
        {
            Console.WriteLine("Invalid selection. Auto-seed selected by default.");
            tempSeasonSettings["College_Field"] = "Load Top 64 Teams (Auto-seed)";
        }
    }

    private void DefineSeriesFormat()
    {
        Console.WriteLine("\n--- SET SERIES LENGTHS ---");
        Console.WriteLine(" [1] Use one format (Best of 3, 5, or 7)");
        Console.WriteLine(" [2] Use mixed format (Set length per round)");
        
        Console.Write("Enter selection: ");
        string input = Console.ReadKey(true).KeyChar.ToString();
        Console.WriteLine(input); 

        if (input == "1")
        {
            Console.Write("Best of: [3], [5], or [7]? ");
            string length = Console.ReadKey(true).KeyChar.ToString();
            tempSeasonSettings["Playoff_Series_Format"] = $"Best of {length}";
        }
        else if (input == "2")
        {
            Console.WriteLine("\nNote: Mixed format logic would prompt for each round length here.");
            tempSeasonSettings["Playoff_Series_Format"] = "Mixed Format (Manual input required)";
        }
        else
        {
            tempSeasonSettings["Playoff_Series_Format"] = "Not Set";
        }
    }

    private void SaveSeason()
    {
        Console.WriteLine("\n--- SAVE SEASON ---");
        
        bool isComplete = 
            tempSeasonSettings.ContainsKey("SeasonLength") && tempSeasonSettings["SeasonLength"] != "Not Set" &&
            tempSeasonSettings.ContainsKey("TeamStructure") && tempSeasonSettings["TeamStructure"] != "Not Set" &&
            tempSeasonSettings.ContainsKey("TeamControl") && tempSeasonSettings["TeamControl"] != "Not Set" &&
            tempSeasonSettings.ContainsKey("Playoffs") && tempSeasonSettings["Playoffs"] != "Not Set";

        if (!isComplete)
        {
            Console.WriteLine("Error: Please complete all 4 Season Setup steps before saving (Length, Teams, Control, Playoffs). Press any key to continue.");
            Console.ReadKey(true);
            return;
        }
        
        Console.Write("Enter Season Name to Save: ");
    string seasonName = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(seasonName))
        {
            Console.WriteLine("Save cancelled: Season name cannot be empty.");
            return;
        }
        
        _currentSeasonState.SetupSettings = tempSeasonSettings; 
        
        if (_dataService.SaveSeason(_currentSeasonState, seasonName))
        {
            Console.WriteLine($"Season '{seasonName}' saved successfully! Returning to Main Menu.");
            
            tempSeasonSettings = new Dictionary<string, string>();
            _currentSeasonState = new SeasonState();
            _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
        }
        else
        {
            Console.WriteLine("Save operation failed. See error log above. Press any key to continue.");
            Console.ReadKey(true);
        }
    }

    #endregion

    #region Load/Delete Season

    private void HandleLoadSeason()
    {
        var savedSeasons = _dataService.GetSavedSeasons();
        
        Console.WriteLine("\n--- LOAD SEASON ---");

        if (savedSeasons.Count == 0)
        {
            Console.WriteLine("No saved seasons found. Press any key to continue.");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("Available Seasons:");
        for (int i = 0; i < savedSeasons.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {savedSeasons[i]}");
        }

        Console.Write("Enter the NUMBER of the season to load, or [B]ack: ");
        // FIX: The input key must be read here to process the selection
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        _engine.PlayClickSound(); // Play click sound after selection

        if (input == "B") return;

        if (int.TryParse(input, out int selectionIndex) && selectionIndex > 0 && selectionIndex <= savedSeasons.Count)
        {
            string selectedName = savedSeasons[selectionIndex - 1];
            SeasonState? loadedState = _dataService.LoadSeason(selectedName);
            
            if (loadedState != null)
            {
                _currentSeasonState = loadedState;
                Console.WriteLine($"\nSeason '{selectedName}' loaded successfully!");
                
                // Immediately transition back to the main menu after a successful load
                Console.WriteLine("Press any key to return to Main Menu...");
                Console.ReadKey(true);
                _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
                return;
            }
        }
        
        // If parsing fails or selection is invalid:
        Console.WriteLine("\nInvalid selection or load failed. Press any key to continue.");
        Console.ReadKey(true);
    }

    private void HandleDeleteSeason()
    {
        var savedSeasons = _dataService.GetSavedSeasons();
        
        Console.WriteLine("\n--- DELETE SEASON ---");
        if (savedSeasons.Count == 0)
        {
            Console.WriteLine("No saved seasons found to delete. Press any key to continue.");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("Select Season to Delete:");
        for (int i = 0; i < savedSeasons.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {savedSeasons[i]}");
        }
        Console.Write("Enter the number of the season to delete, or [B]ack: ");
        
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        _engine.PlayClickSound(); // Play click sound after selection

        if (input == "B") return;

        if (int.TryParse(input, out int selectionIndex) && selectionIndex > 0 && selectionIndex <= savedSeasons.Count)
        {
            string selectedName = savedSeasons[selectionIndex - 1];
            
            // --- TWO-STEP WARNING SYSTEM ---
            Console.WriteLine($"\nWarning: Are You Sure You Want To Delete '{selectedName}'? (Y/N)");
            string confirm1 = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            if (confirm1 != "Y") return;

            Console.WriteLine("\nWarning: Are You Really Really Really Certain? (Y/N)");
            string confirm2 = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            if (confirm2 == "Y")
            {
                if (_dataService.DeleteSeasonFile(selectedName))
                {
                    Console.WriteLine($"\nSeason '{selectedName}' file deleted successfully.");
                }
            }
        }
        Console.WriteLine("Press any key to continue.");
        Console.ReadKey(true);
    }

    #endregion
}
