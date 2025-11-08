using YourSimProject.Models;
using YourSimProject.Services;
using System;
using System.IO;
// Removed EPPlus dependency; using ClosedXML for Excel operations now.
using YourSimProject; // For GameLoopScreen and other screens in namespace

public class Program
{
    // --- NEW: Set EPPlus License before any other class uses it ---
    // This resolves the persistent runtime license error.
    // EPPlus 8+ license assignment removed. Set license via configuration or environment variable if needed.
    // ------------------------------------------------------------------

    private static readonly GameEngine Engine = new();
    
    // Instantiate Core Services
    private static readonly YourSimProject.Services.TeamDatabase TeamDB = new(); 
    private static readonly YourSimProject.Services.SeasonDataService DataService = new();

    // Instantiate Screen Handlers 
    private static readonly SeasonScreen SeasonHandler = new(Engine);
    private static readonly YourSimProject.SettingsScreen SettingsHandler = new(Engine, TeamDB);
    private static readonly YourSimProject.ExhibitionGameScreen ExhibitionHandler = new(Engine, TeamDB);
    private static readonly YourSimProject.UploadScreen UploadHandler = new(Engine, TeamDB);
    private static readonly LoadSaveScreen LoadSaveHandler = new(Engine, DataService);


    public static void Main(string[] args)
    {
        // Initial setup for the console window
        Console.Title = "C# Text Baseball Sim";

        // CLI export format switch: --export-format=csv or --export-format=xlsx
        var exportArg = args.FirstOrDefault(a => a.StartsWith("--export-format=", StringComparison.OrdinalIgnoreCase));
        if (exportArg != null)
        {
            var value = exportArg.Split('=')[1].Trim().ToLowerInvariant();
            if (value == "csv") Engine.ExportFormat = YourSimProject.Models.ExportFormat.Csv;
            else if (value == "xlsx") Engine.ExportFormat = YourSimProject.Models.ExportFormat.Xlsx;
        }
        // Support short flags --csv / --xlsx as well
        if (args.Any(a => a.Equals("--csv", StringComparison.OrdinalIgnoreCase)))
            Engine.ExportFormat = YourSimProject.Models.ExportFormat.Csv;
        if (args.Any(a => a.Equals("--xlsx", StringComparison.OrdinalIgnoreCase)))
            Engine.ExportFormat = YourSimProject.Models.ExportFormat.Xlsx;

        // Apply writer factory immediately based on chosen export format
        YourSimProject.Services.ExcelBuilder.DefaultWriterFactory = () => Engine.ExportFormat == YourSimProject.Models.ExportFormat.Csv
            ? new YourSimProject.Services.CsvWorkbookWriter() as YourSimProject.Services.IWorkbookWriter
            : new YourSimProject.Services.ClosedXmlWorkbookWriter();

        // If any CLI export flag was supplied, persist it to settings.json right away
        if (exportArg != null || args.Any(a => a.Equals("--csv", StringComparison.OrdinalIgnoreCase)) || args.Any(a => a.Equals("--xlsx", StringComparison.OrdinalIgnoreCase)))
        {
            SettingsHandler.PersistExportFormat(Engine.ExportFormat);
        }

        // Optional non-interactive smoke test: runs a single exhibition simulation in "Simulation Only" mode.
        if (args.Any(a => a.Equals("--smoke", StringComparison.OrdinalIgnoreCase)))
        {
            RunSmokeTest();
            return;
        }

        // Optional: run lightweight Excel parser/builder tests (no external framework required)
        if (args.Any(a => a.Equals("--excel-tests", StringComparison.OrdinalIgnoreCase)))
        {
            ExcelStructureParserTests.Run();
            ExcelBuilderSmokeTest.Run();
            return;
        }

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

    private static void RunSmokeTest()
    {
        Console.WriteLine("[SMOKE] Starting automated smoke test (Exhibition, Simulation Only)...");
        // Refresh teams similarly to Exhibition screen logic
        try
        {
            Engine.Teams = TeamDB.Conferences
                .SelectMany(c => c.Regions)
                .SelectMany(r => r.Teams)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMOKE][ERROR] Failed loading teams from TeamDatabase: {ex.Message}");
        }

        var teams = Engine.Teams ?? new List<YourSimProject.Models.Team>();
        if (teams.Count < 2)
        {
            Console.WriteLine("[SMOKE][WARN] Fewer than two teams available. Aborting smoke test.");
            return;
        }

        var teamA = teams[0].Name;
        var teamB = teams[1].Name;
        string location = "Smoke Field";
        string controlType = "Simulation Only";

        Console.WriteLine($"[SMOKE] Matchup: {teamA} vs {teamB} @ {location}");
        var loop = new GameLoopScreen(Engine);
        loop.StartExhibitionGame(teamA, teamB, location, controlType);
        Console.WriteLine("[SMOKE] Simulation completed.");
    }
}
