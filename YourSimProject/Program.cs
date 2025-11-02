using System;
using System.IO;
using OfficeOpenXml; // Needed for the static license setter

public class Program
{
    // --- NEW: Set EPPlus License before any other class uses it ---
    // This resolves the persistent runtime license error.
    // EPPlus 8+ license assignment removed. Set license via configuration or environment variable if needed.
    // ------------------------------------------------------------------

    private static readonly GameEngine Engine = new();
    
    // Instantiate Core Services
    private static readonly TeamDatabase TeamDB = new(); 
    private static readonly SeasonDataService DataService = new();

    // Instantiate Screen Handlers 
    private static readonly SeasonScreen SeasonHandler = new(Engine);
    private static readonly SettingsScreen SettingsHandler = new(Engine, TeamDB);
    private static readonly ExhibitionGameScreen ExhibitionHandler = new(Engine);
    private static readonly UploadScreen UploadHandler = new(Engine, TeamDB);
    private static readonly LoadSaveScreen LoadSaveHandler = new(Engine, DataService);


    public static void Main(string[] args)
    {
        // Initial setup for the console window
        Console.Title = "C# Text Baseball Sim";

        while (true)
        {
            switch (Engine.CurrentScreen)
            {
                case GameEngine.SCREEN_MAIN_MENU:
                    DisplayMainMenu();
                    HandleMainMenuInput();
                    break;

                case GameEngine.SCREEN_SEASON:
                    SeasonHandler.DisplayAndHandle();
                    break;

                case GameEngine.SCREEN_SETTINGS:
                    SettingsHandler.DisplayAndHandle();
                    break;
                
                case GameEngine.SCREEN_EXHIBITION:
                    ExhibitionHandler.DisplayAndHandle();
                    break;

                case GameEngine.SCREEN_UPLOAD:
                    UploadHandler.DisplayAndHandle();
                    break;

                case GameEngine.SCREEN_LOAD_SAVE:
                    LoadSaveHandler.DisplayAndHandle();
                    break;
            }
        }
    }

    private static void DisplayMainMenu()
    {
        Console.Clear();

        Console.WriteLine("\n================================================");
        Console.WriteLine("    BASEBALL SIMULATION: THE MAIN MENU");
        Console.WriteLine("================================================");
        
        // Buttons based on your field layout
    Console.WriteLine(" [P]Pitcher's Mound: Load Last Save");
        Console.WriteLine("------------------------------------------------");
        
        Console.WriteLine(" [1]st Base: Season");
        Console.WriteLine(" [2]nd Base: Upload");
        Console.WriteLine(" [3]rd Base: Exhibition Game");
        Console.WriteLine(" [H]ome Plate: Settings");
        Console.WriteLine("------------------------------------------------");
        Console.Write("Enter selection (1, 2, 3, H, P) or [E]xit: ");
    }

    private static void HandleMainMenuInput()
    {
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        
        switch (input)
        {
            case "1": Engine.NavigateTo(GameEngine.SCREEN_SEASON); break;
            case "2": Engine.NavigateTo(GameEngine.SCREEN_UPLOAD); break;
            case "3": Engine.NavigateTo(GameEngine.SCREEN_EXHIBITION); break;
            case "H": Engine.NavigateTo(GameEngine.SCREEN_SETTINGS); break;
            case "P": Engine.NavigateTo(GameEngine.SCREEN_LOAD_SAVE); break;
            case "E": Environment.Exit(0); break;
            default: Console.WriteLine("\nInvalid selection. Try again."); break;
        }
    }
}
