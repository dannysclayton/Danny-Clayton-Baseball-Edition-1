using System;
using System.Collections.Generic;
using System.Linq;
using YourSimProject.Services;

public class LoadSaveScreen
{
    private readonly GameEngine _engine;
    private readonly SeasonDataService _dataService;

    public LoadSaveScreen(GameEngine engine, SeasonDataService dataService)
    {
        _engine = engine;
        _dataService = dataService;
    }

    public void DisplayAndHandle()
    {
        Console.Clear();
        Console.WriteLine("\n--- PITCHER'S MOUND: LOAD LAST SAVE ---");

    var savedSeasons = _dataService.GetSavedSeasons();

        if (savedSeasons.Count == 0)
        {
            Console.WriteLine("No saved seasons or games found. Cannot load.");
        }
        else
        {
            // The Load Last Save button should attempt to find the most recently modified file.
            // Since our current list of savedSeasons doesn't contain modification times,
            // we will simulate finding the first one and confirm.

            // In a real application, you would find the file with the newest FileInfo.LastWriteTime.
            string? latestSaveName = savedSeasons.FirstOrDefault(); 

            if (string.IsNullOrWhiteSpace(latestSaveName))
            {
                Console.WriteLine("No recent save could be determined.");
                Console.WriteLine("Press [B] to return to the Main Menu.");
                HandleReturnToMain();
                return;
            }

            Console.WriteLine($"Found latest saved game: {latestSaveName}");
            Console.Write($"Load this game and resume play? (Y/N): ");
            
            if (Console.ReadKey(true).KeyChar.ToString().ToUpper() == "Y")
            {
                // Attempt to load the state
                var loadedState = _dataService.LoadSeason(latestSaveName);

                if (loadedState != null)
                {
                    Console.WriteLine($"\n[SUCCESS] Game '{latestSaveName}' loaded.");
                    
                    // This is where you would launch the GameLoopScreen using the loadedState data
                    // For now, we simulate the launch and return.
                    Console.WriteLine("[PLACEHOLDER] Launching game loop with loaded state...");
                }
            }
        }

        Console.WriteLine("\nPress [B] to return to the Main Menu.");
        HandleReturnToMain();
    }
    
    private void HandleReturnToMain()
    {
        if (Console.ReadKey(true).KeyChar.ToString().ToUpper() == "B")
        {
            _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
        }
    }
}
