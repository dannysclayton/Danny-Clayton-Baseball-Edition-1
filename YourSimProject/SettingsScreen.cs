using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using System.Text.Json; 

// FIX: Define the missing helper class for JSON serialization here.
public class SettingsFileStructure
{
    // These properties must match the global config classes
    public AudioConfig AudioConfig { get; set; }
    public SystemConfig SystemConfig { get; set; }
    // Add other persistent global settings here if needed later
}
// END FIX

// NOTE: This class assumes the existence of the AudioConfig, SystemConfig, TeamDatabase, 
//       Conference, Region, Team, Player, PlayerBattingStats, PlayerPitchingStats, 
//       and PlayerFieldingStats models in your project.
public class SettingsScreen
{
    private readonly GameEngine _engine;
    private readonly TeamDatabase _teamDatabase; 
    
    // NOTE: In a full app, these configs would be loaded/saved via a JSON service
    private AudioConfig _audioConfig = new(); 
    private SystemConfig _systemConfig = new(); 

    // Configuration File Path
    private const string SETTINGS_FILE_PATH = "settings.json";

    // Constants for the Settings Menu options
    private const string EDITOR = "Editor";
    private const string AUDIO_MANAGER = "Audio Manager";
    private const string IMAGE_MANAGER = "Image Manager";
    private const string SYSTEM_PREFERENCES = "System Preferences";

    public SettingsScreen(GameEngine engine, TeamDatabase teamDatabase)
    {
        _engine = engine;
        _teamDatabase = teamDatabase; // Store the database reference
        LoadSettings(); // Attempt to load saved settings at startup
        _teamDatabase.LoadDatabase(); // FIX: Ensure we load the League Database on app startup
    }

    /// <summary>
    /// Loads configuration files from disk or initializes defaults.
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SETTINGS_FILE_PATH))
            {
                string jsonString = File.ReadAllText(SETTINGS_FILE_PATH);
                // SettingsFileStructure is assumed to contain AudioConfig and SystemConfig
                var savedSettings = JsonSerializer.Deserialize<SettingsFileStructure>(jsonString);
                
                if (savedSettings != null)
                {
                    _audioConfig = savedSettings.AudioConfig;
                    _systemConfig = savedSettings.SystemConfig;
                    // Console.WriteLine("[INFO] Settings loaded successfully.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to load settings: {ex.Message}");
        }
        // Initialize defaults if load fails
        _audioConfig = new AudioConfig();
        _systemConfig = new SystemConfig();
    }
    
    /// <summary>
    /// Saves ALL persistent data: Settings/Config and the entire Team Database.
    /// </summary>
    private void SaveAllData()
    {
        bool configSaved = false;
        bool dbSaved = false;
        
        // 1. Save Settings (Audio/System Config)
        try
        {
            var settingsToSave = new SettingsFileStructure
            {
                AudioConfig = _audioConfig,
                SystemConfig = _systemConfig,
                // TeamDB serialization would be complex and is deferred
            };
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(settingsToSave, options);
            File.WriteAllText(SETTINGS_FILE_PATH, jsonString);
            configSaved = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to save CONFIG settings: {ex.Message}");
        }

        // 2. Save Team Database (Structure, Teams, Players, Coaches)
        if (_teamDatabase.SaveDatabase())
        {
            dbSaved = true;
        }
        
        if (configSaved && dbSaved)
        {
            Console.WriteLine("\n[SUCCESS] ALL DATA SAVED successfully (Config and League Structure).");
        }
        else
        {
            Console.WriteLine("\n[ALERT] Data save completed with errors (see above).");
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    /// <summary>
    /// Handles the user request to load saved settings and database data.
    /// </summary>
    private void HandleLoadSettings()
    {
        Console.Clear();
        Console.WriteLine("\n--- LOADING SAVED DATA ---");
        
        // 1. Load Config (Audio/System)
        LoadSettings();
        
        // 2. Load Team Database (Structure, Teams, Players, Coaches)
        if (_teamDatabase.LoadDatabase())
        {
            int loadedConfCount = _teamDatabase.Conferences.Count;
            int loadedTeamCount = _teamDatabase.Conferences.Sum(c => c.Regions.Sum(r => r.Teams.Count));
            int loadedPlayerCount = _teamDatabase.GetAllPlayers().Count;

            Console.WriteLine($"[SUCCESS] Configuration loaded.");
            Console.WriteLine($"[SUCCESS] League structure restored: {loadedConfCount} Conferences, {loadedTeamCount} Teams, and {loadedPlayerCount} Players loaded.");
        }
        else
        {
            Console.WriteLine("[INFO] No league data found to load. Starting with current in-memory data.");
        }
        
        Console.WriteLine("\n[SUCCESS] Configuration load process complete.");

        Console.Write("Press any key to return to Settings...");
        Console.ReadKey(true);
    }


    public void DisplayAndHandle()
    {
        Console.Clear();
        Console.WriteLine("\n--- HOME PLATE: SETTINGS HUB ---");
        
        Console.WriteLine($" [1] {EDITOR}");
        Console.WriteLine($" [2] {AUDIO_MANAGER}");
        Console.WriteLine($" [3] {IMAGE_MANAGER}");
        Console.WriteLine($" [4] {SYSTEM_PREFERENCES}");
        Console.WriteLine($" [S] Save All Data"); 
        Console.WriteLine($" [L] Load Saved Data");
        Console.WriteLine($" [B]ack to Main Menu");

        Console.Write("Select an option: ");
        string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        _engine.PlayClickSound();
        Console.WriteLine();

        switch (input)
        {
            case "1":
                HandleEditorMenu();
                break;
            case "2":
                HandleAudioManager();
                break;
            case "3":
                HandleImageManager();
                break;
            case "4":
                HandleSystemPreferences();
                break;
            case "S": 
                SaveAllData(); 
                break;
            case "L": 
                HandleLoadSettings(); 
                break;
            case "B":
                _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
                return;
            default:
                Console.WriteLine("Invalid selection.");
                break;
        }
        
        // Loop back to the Settings main menu 
        if (input != "B" && input.Length == 1 && char.IsLetterOrDigit(input[0])) 
        {
            Console.Clear();
            DisplayAndHandle();
        }
    }

    // --- 1. Editor Implementation (Fully Functional Menu Loop) ---
    private void HandleEditorMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- EDITOR: ASSET MANAGEMENT ---");
            Console.WriteLine($"--- Teams Loaded: {_teamDatabase.Conferences.Sum(c => c.Regions.Sum(r => r.Teams.Count))} ---"); 
            Console.WriteLine("Select Asset to Create/Edit:");
            
            Console.WriteLine(" [C]reate/Edit Conferences"); 
            Console.WriteLine(" [R]eate/Edit Regions"); 
            Console.WriteLine(" [T]eams (Roster, Coach, Stats)"); 
            Console.WriteLine(" [P]layers (Individual Stats)"); // P for Player
            Console.WriteLine(" [O]aches"); // O for Coach (using O to avoid conflict with C)
            Console.WriteLine(" [B]ack to Settings Menu");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            switch (input)
            {
                case "C": HandleAssetEditor("CONFERENCE"); break;
                case "R": HandleAssetEditor("REGION"); break;
                case "T": HandleAssetEditor("TEAM"); break;
                case "P": HandleAssetEditor("PLAYER"); break;
                case "O": HandleAssetEditor("COACH"); break;
                case "B": return; 
                default: Console.WriteLine("Invalid selection."); break;
            }
            
            Console.Write("\nPress any key to return to the Editor menu...");
            Console.ReadKey(true);
        }
    }
    
    private void HandleAssetEditor(string assetType)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- CREATE/EDIT {assetType.ToUpper()} ---");
            
            // --- DISPLAY EXISTING ASSETS ---
            List<object> assets = new List<object>();
            if (assetType == "CONFERENCE")
            {
                assets.AddRange(_teamDatabase.Conferences);
                Console.WriteLine($"Existing Conferences ({assets.Count}):");
            }
            else if (assetType == "REGION")
            {
                assets.AddRange(_teamDatabase.Conferences.SelectMany(c => c.Regions).ToList());
                Console.WriteLine($"Existing Regions ({assets.Count}):");
            }
            else if (assetType == "TEAM")
            {
                assets.AddRange(_teamDatabase.Conferences.SelectMany(c => c.Regions).SelectMany(r => r.Teams).ToList());
                Console.WriteLine($"Existing Teams ({assets.Count}):");
            }
            else if (assetType == "PLAYER")
            {
                // Group players by team for editing
                var teams = _teamDatabase.Conferences.SelectMany(c => c.Regions).SelectMany(r => r.Teams).ToList();
                Console.WriteLine($"Teams ({teams.Count}):");
                for (int i = 0; i < teams.Count; i++)
                {
                    Console.WriteLine($" [{i + 1}] {teams[i].Name} ({teams[i].RegionName})");
                }
                Console.Write("\nSelect a team by ID to view/edit its players, or [B]ack: ");
                string teamInput = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (teamInput == "B") return;
                if (int.TryParse(teamInput, out int teamIndex) && teamIndex > 0 && teamIndex <= teams.Count)
                {
                    var selectedTeam = teams[teamIndex - 1];
                    var teamPlayers = selectedTeam.Roster;
                    if (teamPlayers.Count == 0)
                    {
                        Console.WriteLine($"\nNo players on this team. Press any key to continue...");
                        Console.ReadKey(true);
                        return;
                    }
                    Console.WriteLine($"\nPlayers on {selectedTeam.Name}:");
                    for (int i = 0; i < teamPlayers.Count; i++)
                    {
                        var p = teamPlayers[i];
                        Console.WriteLine($" [{i + 1}] {p.Name} ({p.ClassLevel} - {string.Join("/", p.Positions)})");
                    }
                    Console.Write("\nOptions: [E]dit Player by ID | [B]ack: ");
                    string playerInput = Console.ReadLine()?.Trim().ToUpper() ?? "";
                    if (playerInput == "B") return;
                    if (playerInput == "E")
                    {
                        Console.Write("Enter player ID to edit: ");
                        if (int.TryParse(Console.ReadLine(), out int playerIndex) && playerIndex > 0 && playerIndex <= teamPlayers.Count)
                        {
                            HandlePlayerDetailEditor(teamPlayers[playerIndex - 1]);
                        }
                        else
                        {
                            Console.WriteLine("Invalid player ID. Press any key to continue...");
                            Console.ReadKey(true);
                        }
                    }
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid team ID. Press any key to continue...");
                    Console.ReadKey(true);
                    return;
                }
            }
            else if (assetType == "COACH")
            {
                 // Fetch placeholder coaches
                 assets.AddRange(_teamDatabase.GetAllCoaches());
                 Console.WriteLine($"Existing Coaches ({assets.Count}):");
            }

            if (assets.Count == 0 && assetType != "COACH")
            {
                Console.WriteLine($" [None Loaded/Created.]");
            }
            else
            {
                for (int i = 0; i < assets.Count; i++)
                {
                    string name = GetAssetName(assets[i]);
                    // Display Team ID (index) and Name
                    Console.WriteLine($" [{i + 1}] {name}");
                }
            }
            
            // --- DISPLAY OPTIONS ---
            Console.WriteLine("\nOptions:");
            Console.WriteLine(" [A]dd New | [E]dit Existing (by ID) | [B]ack");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();
            
            switch (input)
            {
                case "A":
                    HandleAddAsset(assetType);
                    break;
                case "E":
                    HandleEditAssetSelection(assetType, assets); // New universal selector
                    break;
                case "B":
                    return; // Go back to the main Editor menu
                default:
                    Console.WriteLine("\nInvalid selection.");
                    break;
            }
        }
    }

    private string GetAssetName(object asset)
    {
        // Helper method to pull the name property regardless of asset type
        if (asset is Conference c) return $"{c.Name} ({c.Regions.Count} Regions)";
        if (asset is Region r) return $"{r.Name} ({r.ConferenceName})"; // Added Region name display
        if (asset is Team t) return $"{t.Name} ({t.RegionName})";
        if (asset is Player p) return $"{p.Name} ({p.ClassLevel} - {string.Join("/", p.Positions)})";
        if (asset is Coach co) return $"{co.Name} ({co.Level})";
        return "Unknown Asset";
    }

    private void HandleEditAssetSelection(string assetType, List<object> assets)
    {
        if (assets.Count == 0)
        {
            Console.WriteLine($"\nNo {assetType}s available to edit. Press any key to continue...");
            Console.ReadKey(true);
            return;
        }

        Console.Write($"\nEnter the ID number of the {assetType} to edit: ");
        
        // FIX: Read the line input to allow multi-digit IDs
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= assets.Count)
        {
            object selectedAsset = assets[index - 1];
            
            switch (assetType)
            {
                case "CONFERENCE":
                    HandleEditConference((Conference)selectedAsset); 
                    break;
                case "REGION":
                    HandleEditRegion((Region)selectedAsset); 
                    break;
                case "TEAM":
                    HandleTeamDetailEditor((Team)selectedAsset);
                    break;
                case "PLAYER":
                    HandlePlayerDetailEditor((Player)selectedAsset); 
                    break;
                case "COACH":
                    HandleCoachDetailEditor((Coach)selectedAsset);
                    break;
            }
        }
        else
        {
            Console.WriteLine("\nInvalid ID selected. Press any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    // --- Team Detail Handlers ---

    // --- Assign/Edit Coach Sub-Menu ---
    private void HandleAssignCoach(Team team)
    {
        var coaches = _teamDatabase.GetAllCoaches();

        while (true)
            {
            Console.Clear();
            Console.WriteLine($"\n--- ASSIGNING COACH TO: {team.Name} ---");
            Console.WriteLine($" Current Coach: {team.Coach?.Name ?? "Unassigned"}");
            
            if (coaches.Count == 0)
            {
                Console.WriteLine("\n[ERROR] No coaches available. Create a new coach first (O).");
                Console.Write("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }
            
            Console.WriteLine("\nAvailable Coaches:");
            for (int i = 0; i < coaches.Count; i++)
            {
                Console.WriteLine($" [{i + 1}] {coaches[i].Name} (Level: {coaches[i].Level})");
            }
            Console.WriteLine(" [B] Back to Team Editor");

            Console.Write("Enter the ID number of the coach to assign or [B]: ");
            string input = Console.ReadLine();

            if (input.ToUpper() == "B") return;

            if (int.TryParse(input, out int index) && index > 0 && index <= coaches.Count)
            {
                Coach selectedCoach = coaches[index - 1];
                team.Coach = selectedCoach; // Assign the coach to the team
                Console.WriteLine($"\n[SUCCESS] Coach {selectedCoach.Name} assigned to {team.Name}.");
            }
            else
            {
                Console.WriteLine("\nInvalid ID selected.");
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    // --- New Coach Editor Sub-Menu ---
    private void HandleCoachDetailEditor(Coach coach)
    {
        // Define options based on your plan
        string[] levels = { "Bad", "Bearable", "Average", "Above Average", "Exceptional" };
        string[] styles = { "Aggressive", "Conservative", "Run of the Mill" };

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING COACH: {coach.Name} ---");
            Console.WriteLine($" [1] Name: {coach.Name}");
            Console.WriteLine($" [2] Level: {coach.Level} (Modifier: {coach.DecisionModifier * 100}%)");
            Console.WriteLine($" [3] Style: {coach.Style}");
            Console.WriteLine($" [4] Experience: {coach.Experience}");
            Console.WriteLine(" [B] Back to Coach List");

            Console.Write("Select attribute to edit (1-4) or [B]: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "B") return;

            Console.Write("\nEnter new value: ");
            string newValue = Console.ReadLine();

            switch (input)
            {
                case "1": 
                    coach.Name = newValue; 
                    break;
                case "2":
                    // Input validation for Level
                    if (levels.Any(l => l.Equals(newValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        coach.Level = newValue;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Level. Options: {string.Join(", ", levels)}.");
                    }
                    break;
                case "3":
                    // Input validation for Style
                    if (styles.Any(s => s.Equals(newValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        coach.Style = newValue;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Style. Options: {string.Join(", ", styles)}.");
                    }
                    break;
                case "4":
                    if (double.TryParse(newValue, out double exp)) coach.Experience = exp;
                    else Console.WriteLine("Invalid input. Experience must be a number.");
                    break;
                default: Console.WriteLine("Invalid selection."); break;
            }
            Console.WriteLine("\n[UPDATE] Attribute modified. Press any key...");
            Console.ReadKey(true);
        }
    }
    // --- End Coach Editor Sub-Menu ---
    
    // --- Conference/Region/Asset Editors ---
    private void HandleEditConference(Conference conference)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING CONFERENCE: {conference.Name} ---");
            Console.WriteLine($" [1] Name: {conference.Name}");
            Console.WriteLine($" [2] Regions: {conference.Regions.Count}");
            Console.WriteLine(" [B] Back to Conference List");

            Console.Write("Select attribute to edit (1) or [B]: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "B") return;

            if (input == "1")
            {
                Console.Write("\nEnter new name for Conference: ");
                string newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    conference.Name = newName;
                    Console.WriteLine($"\n[UPDATE] Conference name set to '{newName}'.");
                }
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    private void HandleEditRegion(Region region)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING REGION: {region.Name} ---");
            Console.WriteLine($" [1] Name: {region.Name}");
            Console.WriteLine($" [2] Conference: {region.ConferenceName}");
            Console.WriteLine(" [B] Back to Region List");

            Console.Write("Select attribute to edit (1) or [B]: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "B") return;

            if (input == "1")
            {
                Console.Write("\nEnter new name for Region: ");
                string newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    region.Name = newName;
                    Console.WriteLine($"\n[UPDATE] Region name set to '{newName}'.");
                }
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    // --- New Player Editor Sub-Menu ---
    private void HandlePlayerDetailEditor(Player player)
    {
        while(true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING PLAYER: {player.Name} ---");
            Console.WriteLine($" Class: {player.ClassLevel} | Modifier: +{player.ClassModifier}");
            Console.WriteLine($" Positions: {string.Join(", ", player.Positions)}");

            Console.WriteLine("\nSelect Detail to Edit:");
            Console.WriteLine(" [1] Edit Attributes (Class, Ratings)");
            Console.WriteLine(" [B] Back to Player List");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            switch (input)
            {
                case "1": 
                    HandleEditPlayerAttributes(player); 
                    break;
                case "B": return;
                default: Console.WriteLine("\nInvalid selection. Press any key to continue..."); break;
            }
            Console.ReadKey(true);
        }
    }

    // --- NEW: Player Attributes Editor ---
    private void HandleEditPlayerAttributes(Player player)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING ATTRIBUTES: {player.Name} ---");
            
            // Display all editable fields as requested
            Console.WriteLine($" [1] Team: {player.TeamName ?? "Unassigned"}");
            Console.WriteLine($" [2] Player Name: {player.Name}");
            Console.WriteLine($" [3] Class: {player.ClassLevel} (Modifier: +{player.ClassModifier})");
            Console.WriteLine($" [4] Position 1: {player.Positions.ElementAtOrDefault(0) ?? "None"}");
            Console.WriteLine($" [5] Position 2: {player.Positions.ElementAtOrDefault(1) ?? "None"}");
            Console.WriteLine($" [6] Position 3: {player.Positions.ElementAtOrDefault(2) ?? "None"}");
            Console.WriteLine($" [7] Batting Rating: {player.BattingRating}");
            Console.WriteLine($" [8] Pitching Rating: {player.PitchingRating}");
            Console.WriteLine(" [B] Back to Player Editor");

            Console.Write("Select attribute to edit (1-8) or [B]: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "B") return;

            Console.Write("\nEnter new value: ");
            string newValue = Console.ReadLine();
            
            // --- Logic to update the Player Model ---
            switch (input)
            {
                case "1": 
                    // FIX: Implemented Team Assignment Logic
                    HandlePlayerTeamAssignment(player, newValue);
                    break;
                case "2": player.Name = newValue; break;
                case "3": 
                    // Input validation for Class Level (must match Freshman, Sophomore, etc.)
                    if (new[] { "FRESHMAN", "SOPHOMORE", "JUNIOR", "SENIOR" }.Contains(newValue.ToUpper()))
                    {
                        player.ClassLevel = newValue; 
                    } else { Console.WriteLine("Invalid Class. Must be Freshman, Sophomore, Junior, or Senior."); }
                    break;
                case "4": UpdatePlayerPosition(player, 0, newValue); break;
                case "5": UpdatePlayerPosition(player, 1, newValue); break;
                case "6": UpdatePlayerPosition(player, 2, newValue); break;
                case "7": 
                    if (int.TryParse(newValue, out int br) && br >= 1 && br >= 1 && br <= 6) player.BattingRating = br; 
                    else { Console.WriteLine("Invalid Rating. Must be a number between 1 and 6."); }
                    break;
                case "8": 
                    if (int.TryParse(newValue, out int pr) && pr >= 1 && pr >= 1 && pr <= 6) player.PitchingRating = pr; 
                    else { Console.WriteLine("Invalid Rating. Must be a number between 1 and 6."); }
                    break;
                default: Console.WriteLine("Invalid selection."); break;
            }
            Console.WriteLine("\n[UPDATE] Attribute modified. Press any key...");
            Console.ReadKey(true);
        }
    }
    
    // FIX: New Method to handle Team Assignment
    private void HandlePlayerTeamAssignment(Player player, string teamName)
    {
        Team oldTeam = null;
        if (!string.IsNullOrEmpty(player.TeamName))
        {
            oldTeam = _teamDatabase.GetTeam(player.TeamName);
        }

        // 1. Find the new target team
        Team newTeam = _teamDatabase.GetTeam(teamName);

        if (newTeam != null)
        {
            // 2. Remove player from old roster if necessary
            if (oldTeam != null && oldTeam.Roster.Contains(player))
            {
                oldTeam.Roster.Remove(player);
            }
            
            // 3. Assign player to new team and update TeamName property
            if (!newTeam.Roster.Contains(player))
            {
                newTeam.Roster.Add(player);
            }
            player.TeamName = newTeam.Name;

            Console.WriteLine($"\n[SUCCESS] {player.Name} assigned to {newTeam.Name}.");
        }
        else if (teamName.Equals("Unassigned", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(teamName))
        {
            // 4. Handle Unassigned/Free Agent status
            if (oldTeam != null)
            {
                oldTeam.Roster.Remove(player);
            }
            player.TeamName = "Unassigned";
            Console.WriteLine($"\n[SUCCESS] {player.Name} is now a Free Agent (Unassigned).");
        }
        else
        {
            Console.WriteLine($"\n[ERROR] Team '{teamName}' not found in the database. Assignment failed.");
        }
    }
    
    // Helper to safely update a position slot
    private void UpdatePlayerPosition(Player player, int index, string newPosition)
    {
        // Ensure the list has enough slots
        while (player.Positions.Count <= index)
        {
            player.Positions.Add("None");
        }
        player.Positions[index] = newPosition;
    }
    // --- End Player Attributes Editor ---


    // --- Team Detail Sub-Menu ---
    private void HandleTeamDetailEditor(Team team)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- EDITING TEAM: {team.Name} ({team.RegionName}) ---");
            Console.WriteLine($" Coach: {team.Coach?.Name ?? "Unassigned"}");
            Console.WriteLine($" Roster Size: {team.Roster.Count}");
            
            Console.WriteLine("\nSelect Detail to Edit:");
            Console.WriteLine(" [R] Edit Roster (30 Players, Positions, Stats)");
            Console.WriteLine(" [C] Assign/Edit Coach"); // FIX: Name changed to reflect functional change
            Console.WriteLine(" [S] View/Edit Team Stats (Win/Loss, Historical)");
            Console.WriteLine(" [B] Back to Team List");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();
            
            switch (input)
            {
                case "R": 
                    HandleRosterEditor(team); // FIX: Launch Roster Editor
                    break;
                case "C": 
                    HandleAssignCoach(team); // FIX: Launch Coach Assignment Sub-Menu
                    break;
                case "S": 
                    HandleViewTeamStats(team); // FIX: Launch Stat Viewer
                    break;
                case "B":
                    return; // Exit sub-menu
                default:
                    Console.WriteLine("\nInvalid selection.");
                    break;
            }
        }
    }
    // --- End Team Detail Sub-Menu ---

    // --- NEW: Team Stat Viewer ---
    private void HandleViewTeamStats(Team team)
    {
        // NOTE: This assumes the team object has basic stat properties populated
        Console.Clear();
        Console.WriteLine($"\n--- TEAM STATISTICS: {team.Name} ---");
        Console.WriteLine($" Conference: {team.ConferenceName}");
        Console.WriteLine($" Region: {team.RegionName}");
        Console.WriteLine("------------------------------------------");
        
        // --- Simulated/Placeholder Team Stats ---
        // Displaying Placeholder/Simulated Stats
        Console.WriteLine($" WINS/LOSSES: {team.Wins} - {team.Losses}");
        Console.WriteLine($" RUNS SCORED: {team.RunsScored}");
        Console.WriteLine($" RUNS ALLOWED: {team.RunsAllowed}");
        Console.WriteLine($" GAMES BEHIND: {team.GamesBehind}");
        
        Console.WriteLine("\n--- TOP LEADERS (Simulated Data) ---");
        Console.WriteLine($" HR Leader: Babe Ruth (10 HR)");
        Console.WriteLine($" ERA Leader: Sandy Koufax (1.50)");
        
        Console.WriteLine("\n[INFO] Detailed Batting, Pitching, and Fielding stats are available in the Excel export.");
        Console.WriteLine("Press [E] to export team data to Excel (Placeholder).");
        
        Console.Write("Press any key to return to Team Editor...");
        Console.ReadKey(true);
    }


    // --- NEW: Roster Editor Sub-Menu ---
    private void HandleRosterEditor(Team team)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- ROSTER EDITOR: {team.Name} ({team.Roster.Count} Players) ---");

            // Display Roster List
            if (team.Roster.Count == 0)
            {
                Console.WriteLine("\n[ROSTER EMPTY] Use Upload (2) or Add Player (P) to populate.");
            }
            else
            {
                Console.WriteLine(" ID | Class | Name                   | Positions   | Bat | Pit | Redshirt");
                Console.WriteLine("-------------------------------------------------------------------------");
                for (int i = 0; i < team.Roster.Count; i++)
                {
                    Player p = team.Roster[i];
                    string positions = string.Join("/", p.Positions.Where(pos => pos != "None").Take(3));
                    string redshirt = p.IsRedshirted ? "[RS]" : " ";
                    
                    Console.WriteLine($" [{i + 1:D2}] | {p.ClassLevel,-9} | {p.Name,-22} | {positions,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {redshirt}");
                }
            }

            Console.WriteLine("\nOptions:");
                Console.WriteLine(" [E]dit Player (by ID) | [M] Redshirt Management | [F] Auto Fielding Assignments | [A] Manual Fielding Assignment | [O] Auto Batting Order | [N] Manual Batting Order | [V] View Lineup/Rotation | [P] Set Pitching Rotation | [Q] Validate Roster | [B] Back");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            switch (input)
                {
                    case "E":
                        HandleEditPlayerInRoster(team);
                        break;
                    case "M":
                        HandleRedshirtManagement(team);
                        break;
                    case "F":
                        AutoAssignFieldingPositions(team);
                        break;
                    case "A":
                        ManualAssignFieldingPositions(team);
                        break;
                    case "O":
                        AutoAssignBattingOrder(team);
                        break;
                    case "N":
                        ManualAssignBattingOrder(team);
                        break;
                    case "V":
                        ViewLineupAndRotation(team);
                        break;
                    case "P":
                        HandlePitchingRotation(team);
                        break;
                    case "Q":
                        ValidateRoster(team);
                        break;
                    case "B":
                        return;
                    default:
                        Console.WriteLine("\nInvalid selection. Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

    // --- Manual Fielding Assignment ---
    private void ManualAssignFieldingPositions(Team team)
    {
        var positions = new[] { "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF", "DH" };
        var assigned = new Dictionary<string, (Player Starter, Player? Backup)>();
        // Helper: track all roles for each player
        var playerRoles = new Dictionary<Player, List<string>>();
        for (int posIdx = 0; posIdx < positions.Length; posIdx++)
        {
            string pos = positions[posIdx];
            Player? starter = null;
            Player? backup = null;
            // Starter selection
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"\n--- MANUAL FIELDING ASSIGNMENT FOR: {team.Name} ---");
                Console.WriteLine($"Assign [Starter] for position: {pos}");
                Console.WriteLine("--------------------------------------------------------------");
                Console.WriteLine(" ID | Name                   | Positions   | Bat | Pit | Status");
                Console.WriteLine("--------------------------------------------------------------");
                var availablePlayers = team.Roster.Where(p => !p.IsRedshirted).ToList();
                for (int i = 0; i < availablePlayers.Count; i++)
                {
                    var p = availablePlayers[i];
                    string status = "";
                    if (playerRoles.ContainsKey(p))
                        status = string.Join(" ", playerRoles[p]);
                    string positionsStr = string.Join("/", p.Positions.Where(x => x != "None").Take(3));
                    Console.WriteLine($" [{i + 1:D2}] | {p.Name,-22} | {positionsStr,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {status}");
                }
                Console.WriteLine($"\nSelect player by ID (1-{availablePlayers.Count}) for [Starter] {pos}, [R]emove assignment, or [B] to cancel.");
                string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (input == "B") return;
                if (input == "R") {
                    // Remove starter assignment for this position
                    if (assigned.ContainsKey(pos) && assigned[pos].Starter != null) {
                        var prevStarter = assigned[pos].Starter;
                        if (playerRoles.ContainsKey(prevStarter))
                            playerRoles[prevStarter].RemoveAll(r => r == $"Starter {pos}");
                        assigned[pos] = (null!, assigned[pos].Backup);
                        starter = null;
                        Console.WriteLine($"Starter assignment for {pos} removed. Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                    } else {
                        Console.WriteLine($"No starter assigned for {pos}. Press any key to continue...");
                        Console.ReadKey(true);
                    }
                    continue;
                }
                if (int.TryParse(input, out int id) && id > 0 && id <= availablePlayers.Count)
                {
                    starter = availablePlayers[id - 1];
                    if (!playerRoles.ContainsKey(starter)) playerRoles[starter] = new List<string>();
                    if (!playerRoles[starter].Contains($"Starter {pos}"))
                        playerRoles[starter].Add($"Starter {pos}");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid selection. Press any key to retry...");
                    Console.ReadKey(true);
                }
            }
            // Backup selection
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"\n--- MANUAL FIELDING ASSIGNMENT FOR: {team.Name} ---");
                Console.WriteLine($"Assign [Back Up] for position: {pos}");
                Console.WriteLine("--------------------------------------------------------------");
                Console.WriteLine(" ID | Name                   | Positions   | Bat | Pit | Status");
                Console.WriteLine("--------------------------------------------------------------");
                var availablePlayers = team.Roster.Where(p => !p.IsRedshirted).ToList();
                for (int i = 0; i < availablePlayers.Count; i++)
                {
                    var p = availablePlayers[i];
                    string status = "";
                    if (playerRoles.ContainsKey(p))
                        status = string.Join(" ", playerRoles[p]);
                    string positionsStr = string.Join("/", p.Positions.Where(x => x != "None").Take(3));
                    Console.WriteLine($" [{i + 1:D2}] | {p.Name,-22} | {positionsStr,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {status}");
                }
                Console.WriteLine($"\nSelect player by ID (1-{availablePlayers.Count}) for [Back Up] {pos}, [R]emove assignment, [S]kip, or [B] to cancel.");
                string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (input == "B") return;
                if (input == "S") break;
                if (input == "R") {
                    // Remove backup assignment for this position
                    if (assigned.ContainsKey(pos) && assigned[pos].Backup != null) {
                        var prevBackup = assigned[pos].Backup;
                        if (playerRoles.ContainsKey(prevBackup))
                            playerRoles[prevBackup].RemoveAll(r => r == $"Back Up {pos}");
                        assigned[pos] = (assigned[pos].Starter, null);
                        backup = null;
                        Console.WriteLine($"Backup assignment for {pos} removed. Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                    } else {
                        Console.WriteLine($"No backup assigned for {pos}. Press any key to continue...");
                        Console.ReadKey(true);
                    }
                    continue;
                }
                if (int.TryParse(input, out int id) && id > 0 && id <= availablePlayers.Count)
                {
                    backup = availablePlayers[id - 1];
                    if (!playerRoles.ContainsKey(backup)) playerRoles[backup] = new List<string>();
                    if (!playerRoles[backup].Contains($"Back Up {pos}"))
                        playerRoles[backup].Add($"Back Up {pos}");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid selection. Press any key to retry...");
                    Console.ReadKey(true);
                }
            }
            assigned[pos] = (starter!, backup);
        }
        // Preview assignments
        Console.WriteLine("\nManual Fielding Assignment Preview:");
        foreach (var pos in positions)
        {
            if (assigned.ContainsKey(pos))
            {
                var pair = assigned[pos];
                Console.WriteLine($" {pos}: [Starter] {pair.Starter.Name} ({string.Join("/", pair.Starter.Positions)}) Bat: {pair.Starter.BattingRating}");
                if (pair.Backup != null)
                    Console.WriteLine($"     [Back Up] {pair.Backup.Name} ({string.Join("/", pair.Backup.Positions)}) Bat: {pair.Backup.BattingRating}");
            }
            else
            {
                Console.WriteLine($" {pos}: [Not assigned]");
            }
        }
        Console.Write("\nFinalize these assignments? (Y/N): ");
        var confirm = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        if (confirm == "Y")
        {
            team.FieldingAssignments = assigned;
            Console.WriteLine("\n[SUCCESS] Fielding assignments finalized.");
        }
        else
        {
            Console.WriteLine("\n[INFO] Fielding assignment canceled.");
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    // --- Manual Batting Order Selection ---
    private void ManualAssignBattingOrder(Team team)
    {
        if (team.Roster.Count < 9)
        {
            Console.WriteLine("\n[ERROR] Not enough players to set batting order (need at least 9).");
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
        // Only allow selection from finalized fielding assignment starters
        List<Player> starters = new List<Player>();
        Dictionary<string, Player> positionStarters = new Dictionary<string, Player>();
        if (team.FieldingAssignments != null && team.FieldingAssignments.Count > 0)
        {
            foreach (var kvp in team.FieldingAssignments)
            {
                if (kvp.Value.Starter != null)
                {
                    positionStarters[kvp.Key] = kvp.Value.Starter;
                }
            }
            starters = positionStarters.Values.Distinct().ToList();
        }
        else
        {
            starters = team.Roster.Take(9).ToList();
        }

        List<Player> selected = new List<Player>();
        HashSet<int> selectedIndices = new HashSet<int>();
        while (selected.Count < 9)
        {
            Console.Clear();
            Console.WriteLine($"\n--- MANUAL BATTING ORDER FOR: {team.Name} ---");
            Console.WriteLine($" Players Selected: {selected.Count} / 9");
            Console.WriteLine("\nAssigned Position Starters:");
            Console.WriteLine(" Pos | Name                   | Bat | Pit");
            Console.WriteLine("----------------------------------------------");
            foreach (var pos in new[] { "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF", "DH" })
            {
                if (positionStarters.ContainsKey(pos))
                {
                    var p = positionStarters[pos];
                    Console.WriteLine($" {pos,-3} | {p.Name,-22} | {p.BattingRating,-3} | {p.PitchingRating,-3}");
                }
                else
                {
                    Console.WriteLine($" {pos,-3} | [Not assigned]");
                }
            }
            Console.WriteLine("\nSelect batting order from assigned starters below:");
            Console.WriteLine(" ID | Name                   | Positions   | Bat | Pit | Status");
            Console.WriteLine("--------------------------------------------------------------");
            for (int i = 0; i < starters.Count; i++)
            {
                var p = starters[i];
                string positions = string.Join("/", p.Positions.Where(pos => pos != "None").Take(3));
                string status = " ";
                int batterNum = selected.IndexOf(p);
                if (batterNum >= 0)
                {
                    status = $"Batter number {batterNum + 1}";
                }
                Console.WriteLine($" [{i + 1:D2}] | {p.Name,-22} | {positions,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {status}");
            }
            Console.WriteLine("\nSelect player by ID (1-{0}), or [B] to cancel.", starters.Count);
            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (input == "B") return;
            if (int.TryParse(input, out int id) && id > 0 && id <= starters.Count)
            {
                int idx = id - 1;
                if (selectedIndices.Contains(idx))
                {
                    selectedIndices.Remove(idx);
                    selected.Remove(starters[idx]);
                }
                else if (selected.Count < 9)
                {
                    selectedIndices.Add(idx);
                    selected.Add(starters[idx]);
                }
            }
        }
        // Preview order
        Console.WriteLine("\nManual Batting Order Preview:");
        for (int i = 0; i < selected.Count; i++)
        {
            var p = selected[i];
            Console.WriteLine($" {i + 1}: {p.Name} ({string.Join("/", p.Positions)}) Bat: {p.BattingRating}");
        }
        Console.Write("\nFinalize this batting order? (Y/N): ");
        var confirm = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        if (confirm == "Y")
        {
            team.BattingLineup.Clear();
            team.BattingLineup.AddRange(selected);
            Console.WriteLine("\n[SUCCESS] Batting order finalized.");
        }
        else
        {
            Console.WriteLine("\n[INFO] Batting order assignment canceled.");
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    // --- View Lineup and Rotation ---
    private void ViewLineupAndRotation(Team team)
    {
        Console.Clear();
        Console.WriteLine($"\n--- {team.Name} LINEUP & ROTATION ---");

        // Assigned Position Players (Starters)
        Console.WriteLine("\nAssigned Position Starters:");
        Console.WriteLine(" Pos | Name                   | Bat | Pit");
        Console.WriteLine("----------------------------------------------");
        if (team.FieldingAssignments != null && team.FieldingAssignments.Count > 0)
        {
            foreach (var pos in new[] { "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF", "DH" })
            {
                if (team.FieldingAssignments.ContainsKey(pos) && team.FieldingAssignments[pos].Starter != null)
                {
                    var p = team.FieldingAssignments[pos].Starter;
                    Console.WriteLine($" {pos,-3} | {p.Name,-22} | {p.BattingRating,-3} | {p.PitchingRating,-3}");
                }
                else
                {
                    Console.WriteLine($" {pos,-3} | [Not assigned]");
                }
            }
        }
        else
        {
            Console.WriteLine(" [No fielding assignments set]");
        }

        Console.WriteLine("\nBatting Lineup:");
        if (team.BattingLineup.Count == 0)
            Console.WriteLine(" [Not set]");
        else
            for (int i = 0; i < team.BattingLineup.Count; i++)
            {
                var p = team.BattingLineup[i];
                Console.WriteLine($" {i + 1}: {p.Name} ({string.Join("/", p.Positions)}) Bat: {p.BattingRating}");
            }

        Console.WriteLine("\nPitching Rotation:");
        if (team.PitchingRotation.Count == 0)
            Console.WriteLine(" [Not set]");
        else
            for (int i = 0; i < team.PitchingRotation.Count; i++)
            {
                var p = team.PitchingRotation[i];
                Console.WriteLine($" {i + 1}: {p.Name} ({string.Join("/", p.Positions)}) Pit: {p.PitchingRating}");
            }

        Console.Write("\nPress any key to return...");
        Console.ReadKey(true);
    }

    // --- NEW: Pitching Rotation Selection ---
    private void HandlePitchingRotation(Team team)
    {
        var pitchers = team.Roster.Where(p => p.Positions.Any(pos => pos == "P" || pos == "SP" || pos == "RHP" || pos == "LHP" || pos == "RP")).ToList();
        if (pitchers.Count < 5)
        {
            Console.WriteLine("\n[ERROR] Not enough pitchers (need at least 5 with P/SP/RHP/LHP/RP position).");
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }

        List<Player> selected = new List<Player>();
        HashSet<int> selectedIndices = new HashSet<int>();
        while (selected.Count < 5)
        {
            Console.Clear();
            Console.WriteLine($"\n--- SET PITCHING ROTATION FOR: {team.Name} ---");
            Console.WriteLine($" Pitchers Selected: {selected.Count} / 5");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine(" ID | Name                   | Positions   | Bat | Pit | Status");
            Console.WriteLine("--------------------------------------------------------------");
            for (int i = 0; i < pitchers.Count; i++)
            {
                var p = pitchers[i];
                string status = selectedIndices.Contains(i) ? "[SELECTED]" : " ";
                string positions = string.Join("/", p.Positions.Where(pos => pos != "None").Take(3));
                Console.WriteLine($" [{i + 1:D2}] | {p.Name,-22} | {positions,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {status}");
            }
            Console.WriteLine("\nSelect pitcher by ID (1-{0}), or [B] to cancel.", pitchers.Count);
            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (input == "B") return;
            if (int.TryParse(input, out int id) && id > 0 && id <= pitchers.Count)
            {
                int idx = id - 1;
                if (selectedIndices.Contains(idx))
                {
                    selectedIndices.Remove(idx);
                    selected.Remove(pitchers[idx]);
                }
                else if (selected.Count < 5)
                {
                    selectedIndices.Add(idx);
                    selected.Add(pitchers[idx]);
                }
            }
        }
        // Preview rotation
        Console.WriteLine("\nPitching Rotation Preview:");
        for (int i = 0; i < selected.Count; i++)
        {
            var p = selected[i];
            Console.WriteLine($" {i + 1}: {p.Name} ({string.Join("/", p.Positions)}) Pit: {p.PitchingRating}");
        }
        Console.Write("\nFinalize this rotation? (Y/N): ");
        var confirm = Console.ReadKey(true).KeyChar.ToString().ToUpper();
        if (confirm == "Y")
        {
            team.PitchingRotation.Clear();
            team.PitchingRotation.AddRange(selected);
            Console.WriteLine("\n[SUCCESS] Pitching rotation finalized.");
        }
        else
        {
            Console.WriteLine("\n[INFO] Pitching rotation assignment canceled.");
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }
        // --- AI Logic: Auto Fielding Assignments ---
        private void AutoAssignFieldingPositions(Team team)
        {
            Console.WriteLine("\n[AI] Auto-assigning fielding positions...");
            var positions = new[] { "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF", "DH" };
            var assigned = new Dictionary<string, (Player Starter, Player? Backup)>();
            // Track all roles for each player
            var playerRoles = new Dictionary<Player, List<string>>();
            // Assign best matching players for each position
            foreach (var pos in positions)
            {
                var candidates = team.Roster.Where(p => !p.IsRedshirted && p.Positions.Contains(pos)).OrderByDescending(p => p.BattingRating).ToList();
                Player? starter = null;
                Player? backup = null;
                if (candidates.Count > 0)
                {
                    starter = candidates[0];
                    if (!playerRoles.ContainsKey(starter)) playerRoles[starter] = new List<string>();
                    playerRoles[starter].Add($"Starter {pos}");
                    if (candidates.Count > 1)
                    {
                        backup = candidates[1];
                        if (!playerRoles.ContainsKey(backup)) playerRoles[backup] = new List<string>();
                        playerRoles[backup].Add($"Back Up {pos}");
                    }
                }
                else
                {
                    // Out-of-position: pick best available
                    var fallback = team.Roster.Where(p => !p.IsRedshirted).OrderByDescending(p => p.BattingRating).FirstOrDefault(p => !playerRoles.ContainsKey(p) || !playerRoles[p].Contains($"Starter {pos}"));
                    if (fallback != null)
                    {
                        starter = fallback;
                        if (!playerRoles.ContainsKey(starter)) playerRoles[starter] = new List<string>();
                        playerRoles[starter].Add($"Starter {pos}");
                        // Try to find another backup
                        var backupFallback = team.Roster.Where(p => !p.IsRedshirted).OrderByDescending(p => p.BattingRating).FirstOrDefault(p => p != starter && (!playerRoles.ContainsKey(p) || !playerRoles[p].Contains($"Back Up {pos}")));
                        if (backupFallback != null)
                        {
                            backup = backupFallback;
                            if (!playerRoles.ContainsKey(backup)) playerRoles[backup] = new List<string>();
                            playerRoles[backup].Add($"Back Up {pos}");
                        }
                    }
                }
                assigned[pos] = (starter!, backup);
            }
            // Preview assignments
            Console.WriteLine("\nFielding Assignments Preview:");
            foreach (var p in team.Roster)
            {
                if (playerRoles.ContainsKey(p))
                {
                    Console.WriteLine($" {p.Name}: {string.Join(" ", playerRoles[p])} (Bat: {p.BattingRating})");
                }
            }
            Console.Write("\nFinalize these assignments? (Y/N): ");
            var confirm = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            if (confirm == "Y")
            {
                team.FieldingAssignments = assigned;
                Console.WriteLine("\n[INFO] Fielding assignments finalized.");
            }
            else
            {
                Console.WriteLine("\n[INFO] Fielding assignments canceled.");
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        // --- AI Logic: Auto Batting Order ---
        private void AutoAssignBattingOrder(Team team)
        {
            Console.WriteLine("\n[AI] Auto-assigning batting order...");
            var orderSlots = new[] { 3, 4, 2, 1, 5, 6, 7, 8, 9 };
            var sorted = team.Roster.OrderByDescending(p => p.BattingRating).Take(9).ToList();
            var battingOrder = new List<Player>(new Player[9]);
            for (int i = 0; i < sorted.Count && i < orderSlots.Length; i++)
            {
                int slot = orderSlots[i] - 1;
                battingOrder[slot] = sorted[i];
            }
            // Only show preview for starters in finalized fielding assignments
            List<Player> starters = new List<Player>();
            if (team.FieldingAssignments != null && team.FieldingAssignments.Count > 0)
            {
                starters = team.FieldingAssignments.Values
                    .Where(pair => pair.Starter != null)
                    .Select(pair => pair.Starter)
                    .Distinct()
                    .ToList();
            }
            else
            {
                starters = team.Roster.Take(9).ToList();
            }
            Console.WriteLine("\nBatting Order Preview:");
            for (int i = 0; i < starters.Count; i++)
            {
                var p = starters[i];
                Console.WriteLine($" {i + 1}: {p.Name} (Bat: {p.BattingRating})");
            }
            Console.Write("\nFinalize this batting order? (Y/N): ");
            var confirm = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            if (confirm == "Y")
            {
                team.BattingLineup.Clear();
                team.BattingLineup.AddRange(starters);
                Console.WriteLine("\n[INFO] Batting order finalized.");
            }
            else
            {
                Console.WriteLine("\n[INFO] Batting order assignment canceled.");
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    
    private void HandleEditPlayerInRoster(Team team)
    {
        if (team.Roster.Count == 0) return;
        
        Console.Write("\nEnter the ID number of the player to edit: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= team.Roster.Count)
        {
            // Launch the existing player editor sub-menu
            HandlePlayerDetailEditor(team.Roster[index - 1]);
        }
        else
        {
            Console.WriteLine("Invalid player ID.");
        }
    }
    
    private void HandleRedshirtManagement(Team team)
    {
        while (true)
        {
            Console.Clear();
            int currentRedshirts = team.Roster.Count(p => p.IsRedshirted);
            Console.WriteLine($"\n--- REDSHIRT MANAGEMENT: {team.Name} (Limit: 5) ---");
            Console.WriteLine($" Current Redshirts: {currentRedshirts}");
            
            Console.WriteLine("\nOptions:");
            Console.WriteLine(" [A]dd Redshirt | [R]emove Redshirt | [B] Back");
            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            
            if (input == "B") return;

            if (input == "A")
            {
                if (currentRedshirts >= 5)
                {
                    Console.WriteLine("\n[ERROR] Redshirt limit (5) reached.");
                }
                else
                {
                    Console.Write("\nEnter player ID to add to redshirt: ");
                    if (int.TryParse(Console.ReadLine(), out int id) && id > 0 && id <= team.Roster.Count)
                    {
                        Player p = team.Roster[id - 1];
                        if (p.IsRedshirted)
                        {
                            Console.WriteLine($"\n[INFO] {p.Name} is already redshirted.");
                        }
                        else
                        {
                            p.IsRedshirted = true;
                            Console.WriteLine($"\n[SUCCESS] {p.Name} is now redshirted.");
                        }
                    } else Console.WriteLine("\nInvalid player ID.");
                }
            }
            else if (input == "R")
            {
                Console.Write("\nEnter player ID to remove from redshirt: ");
                if (int.TryParse(Console.ReadLine(), out int id) && id > 0 && id <= team.Roster.Count)
                {
                    Player p = team.Roster[id - 1];
                    if (!p.IsRedshirted)
                    {
                        Console.WriteLine($"\n[INFO] {p.Name} is not redshirted.");
                    }
                    else
                    {
                        p.IsRedshirted = false;
                        Console.WriteLine($"\n[SUCCESS] {p.Name} removed from redshirt status.");
                    }
                } else Console.WriteLine("\nInvalid player ID.");
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    private void ValidateRoster(Team team)
    {
        Console.WriteLine("\n--- ROSTER VALIDATION CHECK ---");
        List<string> errors = new List<string>();
        
        // 1. Check Roster Size (Must be 30 or less)
        if (team.Roster.Count < 9) // Minimum size for a functional team (even DH uses 9 hitters + pitchers)
        {
            errors.Add($"Roster Size Error: Roster has only {team.Roster.Count} players (Min 9 for initial setup).");
        }
        
        // 2. Check Pitchers Minimum (Min 13)
        int pitcherCount = team.Roster.Count(p => p.Positions.Contains("P", StringComparer.OrdinalIgnoreCase) || p.Positions.Contains("SP", StringComparer.OrdinalIgnoreCase) || p.Positions.Contains("RP", StringComparer.OrdinalIgnoreCase));
        if (pitcherCount < 13)
        {
            errors.Add($"Pitcher Count Error: Only {pitcherCount} players are Pitcher-qualified (Min 13).");
        }
        
        // 3. Check Position Minimums (Min 3 per position)
        string[] requiredPositions = { "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF" };
        foreach (var pos in requiredPositions)
        {
            int posCount = team.Roster.Count(p => p.Positions.Contains(pos, StringComparer.OrdinalIgnoreCase));
            if (posCount < 3)
            {
                errors.Add($"Position Error: {pos} has only {posCount} qualified players (Min 3).");
            }
        }
        
        // --- Display Results ---
        if (errors.Any())
        {
            Console.WriteLine("\n[FAILED] Roster does not meet minimum league requirements:");
            foreach (var err in errors)
            {
                Console.WriteLine($"- {err}");
            }
        }
        else
        {
            Console.WriteLine("\n[SUCCESS] Roster meets all minimum requirements!");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }
    // --- End Roster Editor Sub-Menu ---


    private void HandleAddAsset(string assetType)
    {
        Console.WriteLine($"\n--- ADD NEW {assetType.ToUpper()} ---");
        Console.Write($"Enter name for new {assetType}: ");
        string name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("[ERROR] Name cannot be empty.");
        }
        else
        {
            if (assetType == "CONFERENCE")
            {
                // FIX: Add new Conference to the database list
                _teamDatabase.Conferences.Add(new Conference { Name = name });
                Console.WriteLine($"[SUCCESS] Conference '{name}' added successfully.");
            }
            else if (assetType == "REGION")
            {
                // Logic for adding a Region: requires selecting a parent Conference
                if (_teamDatabase.Conferences.Count == 0)
                {
                    Console.WriteLine("[ERROR] Cannot add Region: No Conferences exist. Add a Conference first.");
                }
                else
                {
                    Console.WriteLine("\nSelect parent Conference:");
                    for(int i = 0; i < _teamDatabase.Conferences.Count; i++)
                    {
                        Console.WriteLine($" [{i + 1}] {_teamDatabase.Conferences[i].Name}");
                    }
                    Console.Write("Enter Conference ID: ");
                    if (int.TryParse(Console.ReadLine(), out int confIndex) && confIndex > 0 && confIndex <= _teamDatabase.Conferences.Count)
                    {
                        var parentConf = _teamDatabase.Conferences[confIndex - 1];
                        parentConf.Regions.Add(new Region { Name = name, ConferenceName = parentConf.Name });
                        Console.WriteLine($"[SUCCESS] Region '{name}' added to {parentConf.Name}.");
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Invalid Conference ID.");
                    }
                }
            }
            else if (assetType == "TEAM")
            {
                // Logic for adding a Team: requires selecting a parent Region
                var regions = _teamDatabase.Conferences.SelectMany(c => c.Regions).ToList();
                 if (regions.Count == 0)
                {
                    Console.WriteLine("[ERROR] Cannot add Team: No Regions exist. Add a Region first.");
                }
                else
                {
                    Console.WriteLine("\nSelect parent Region:");
                    for(int i = 0; i < regions.Count; i++)
                    {
                        Console.WriteLine($" [{i + 1}] {regions[i].Name} ({regions[i].ConferenceName})");
                    }
                    Console.Write("Enter Region ID: ");
                    if (int.TryParse(Console.ReadLine(), out int regIndex) && regIndex > 0 && regIndex <= regions.Count)
                    {
                        var parentRegion = regions[regIndex - 1];
                        parentRegion.Teams.Add(new Team {
                            Name = name,
                            RegionName = parentRegion.Name,
                            ConferenceName = parentRegion.ConferenceName,
                            Coach = new Coach { Name = "Default Coach", Level = "Average", Style = "Standard" },
                            LogoPath = "default_logo.png",
                            UniformPath = "default_uniform.png",
                            TeamPhotoPath = "default_team_photo.png",
                            TeamFieldingPhotoPath = "default_fielding_photo.png",
                            TeamBattingPhotoPath = "default_batting_photo.png",
                            TeamPitchingPhotoPath = "default_pitching_photo.png"
                        });
                        Console.WriteLine($"[SUCCESS] Team '{name}' added to {parentRegion.Name}.");
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Invalid Region ID.");
                    }
                }
            }
            else if (assetType == "PLAYER")
            {
                // Logic for adding a Player: requires basic attributes
                // NOTE: Player object is created here with minimal defaults
                _teamDatabase.GetAllPlayers().Add(new Player { Name = name, ClassLevel = "Freshman", Positions = new List<string> { "None" } });
                Console.WriteLine($"[SUCCESS] Player '{name}' created. Please edit attributes and assign to a team.");
            }
            else if (assetType == "COACH")
            {
                // NOTE: Coach object is created here with minimal defaults
                _teamDatabase.GetAllCoaches().Add(new Coach { Name = name, Level = "Average", Style = "Run of the Mill" });
                Console.WriteLine($"[SUCCESS] Coach '{name}' added.");
            }
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    private void HandleEditAsset(string assetType)
    {
        Console.Write($"\nEnter the ID number of the {assetType} to edit: ");
        string idInput = Console.ReadLine();
        
        // Placeholder for actual lookup and editing logic
        Console.WriteLine($"[PLACEHOLDER] Editing {assetType} with ID '{idInput}'.");
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    private List<Coach> GetPlaceholderCoaches()
    {
        // FIX: Provide a persistent list of coaches for editing and adding
        if (!_teamDatabase.GetAllCoaches().Any())
        {
            _teamDatabase.GetAllCoaches().AddRange(new List<Coach>
            {
                new Coach { Name = "Coach Taylor", Level = "Exceptional", Style = "Aggressive", Experience = 100 },
                new Coach { Name = "Coach Bell", Level = "Bearable", Style = "Conservative", Experience = 25.5 }
            });
        }
        // NOTE: This relies on the TeamDatabase having a GetAllCoaches method implemented
        return _teamDatabase.GetAllCoaches(); 
    }
    
    // --- 2. Audio Manager Implementation ---
    private void HandleAudioManager()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- 2. AUDIO MANAGER: ASSIGN SOUNDS ---");
            Console.WriteLine("Available Audio Files in 'Audio' folder:");
            var audioFiles = GetAudioFiles();

            if (audioFiles.Count == 0)
            {
                Console.WriteLine("No .wav or .mp3 files found in the 'Audio' folder.");
            }
            else
            {
                for (int i = 0; i < audioFiles.Count; i++)
                {
                    Console.WriteLine($" [{i + 1}] {audioFiles[i]}");
                }
            }
            
            Console.WriteLine("\n--- ASSIGN TO GAME EVENT (Current Settings) ---");
            Console.WriteLine($" [M]enu Click: {_audioConfig.MenuClickSoundPath}");
            Console.WriteLine($" [L]oad Game:  {_audioConfig.PlayBallSoundPath}");
            Console.WriteLine($" [H]ome Run:   {_audioConfig.HomeRunSoundPath}");
            Console.WriteLine($" [S]strikeout:  {_audioConfig.StrikeoutSoundPath}");
            Console.WriteLine(" [B]ack to Settings Menu");

            Console.Write("Enter selection (M, L, H, S) or [B]: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "B") return;

            if (input == "M" || input == "L" || input == "H" || input == "S")
            {
                Console.Write("\nEnter the NUMBER of the audio file to assign: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= audioFiles.Count)
                {
                    string selectedFile = "Audio/" + audioFiles[index - 1];
                    
                    switch (input)
                    {
                        case "M": 
                            _audioConfig.MenuClickSoundPath = selectedFile; 
                            break;
                        case "L": _audioConfig.PlayBallSoundPath = selectedFile; break;
                        case "H": _audioConfig.HomeRunSoundPath = selectedFile; break;
                        case "S": _audioConfig.StrikeoutSoundPath = selectedFile; break;
                    }
                    Console.WriteLine($"\n[SUCCESS] Assigned {audioFiles[index - 1]} to event '{input}'.");
                }
                else
                {
                    Console.WriteLine("\nInvalid file number selected.");
                }
            }
            
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    // --- 3. Image Manager Implementation ---
    private void HandleImageManager()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- 3. IMAGE MANAGER: ASSIGN VISUALS ---");
            Console.WriteLine("Available Image Files in 'Team' folder:");
            
            var imageFiles = GetImageFiles();

            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No .jpg or .png files found in the 'Team' folder.");
            }
            else
            {
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    Console.WriteLine($" [{i + 1}] {imageFiles[i]}");
                }
            }

            Console.WriteLine("\n--- ASSIGN IMAGES ---");
            Console.WriteLine(" [L]ogo to Team | [U]Uniform to Team | [B]Background to Screen");
            Console.WriteLine(" [X]Back to Settings Menu");

            Console.Write("Enter selection (L, U, B, X): ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            if (input == "X") return; 

            switch (input)
            {
                case "L":
                    HandleTeamVisualAssignment("Logo", imageFiles);
                    break;
                case "U":
                    HandleTeamVisualAssignment("Uniform", imageFiles);
                    break;
                case "B":
                    HandleScreenBackgroundAssignment(imageFiles);
                    break;
                default:
                    Console.WriteLine("\nInvalid selection.");
                    break;
            }
            
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }

    // --- 4. System Preferences Implementation ---
    private void HandleSystemPreferences()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- 4. SYSTEM PREFERENCES ---");
            Console.WriteLine("Toggle global settings:");
            
            string soundStatus = _systemConfig.EnableSoundEffects ? "ON" : "OFF";
            string logoStatus = _systemConfig.EnableTeamLogos ? "ON" : "OFF";
            
            Console.WriteLine($" [S] Toggle Sound Effects: {soundStatus}");
            Console.WriteLine($" [L] Toggle Team Logos (Excel/Reports): {logoStatus}");
            Console.WriteLine($" [D] Set Default Save Location (Current: {_systemConfig.DefaultSaveLocation})");
            Console.WriteLine($" [R] Reset All Settings to Default");
            Console.WriteLine(" [B] Back to Settings Menu");

            Console.Write("Enter selection: ");
            string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            _engine.PlayClickSound();

            switch (input)
            {
                case "S":
                    _systemConfig.EnableSoundEffects = !_systemConfig.EnableSoundEffects;
                    Console.WriteLine($"\nSound Effects set to {_systemConfig.EnableSoundEffects}.");
                    break;
                case "L":
                    _systemConfig.EnableTeamLogos = !_systemConfig.EnableTeamLogos;
                    Console.WriteLine($"\nTeam Logos set to {_systemConfig.EnableTeamLogos}.");
                    break;
                case "D":
                    Console.Write($"\nEnter new save location (Current: {_systemConfig.DefaultSaveLocation}): ");
                    string newLocation = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newLocation))
                    {
                        _systemConfig.DefaultSaveLocation = newLocation;
                        Console.WriteLine($"\nSave location updated to '{newLocation}'.");
                    }
                    break;
                case "R":
                    // Resetting involves creating a new instance of the config model
                    _systemConfig.EnableSoundEffects = new SystemConfig().EnableSoundEffects; 
                    _systemConfig.EnableTeamLogos = new SystemConfig().EnableTeamLogos;
                    _systemConfig.DefaultSaveLocation = new SystemConfig().DefaultSaveLocation;
                    Console.WriteLine("\nAll system settings reset to default values.");
                    break;
                case "B":
                    return;
                default:
                    Console.WriteLine("\nInvalid selection.");
                    break;
            }
            
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    // --- Helper Methods (Used by Image Manager) ---
    private void HandleTeamVisualAssignment(string assetType, List<string> imageFiles)
    {
        if (imageFiles.Count == 0) return;
        
        Console.Write($"\nEnter the team name to assign the {assetType}: ");
        string teamName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(teamName))
        {
            Console.WriteLine("Invalid team name.");
            return;
        }

        Console.Write($"Enter the NUMBER of the image file to assign: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= imageFiles.Count)
        {
            string selectedFile = "Team/" + imageFiles[index - 1];
            
            Console.WriteLine($"\n[SUCCESS] Assigned {selectedFile} as {assetType} for {teamName}. (Metadata update placeholder)");
        }
        else
        {
            Console.WriteLine("\nInvalid file number selected.");
        }
    }

    private void HandleScreenBackgroundAssignment(List<string> imageFiles)
    {
        if (imageFiles.Count == 0) return;

    Console.WriteLine("\n--- ASSIGN BACKGROUND ---"); // Already correct, no change needed
        Console.WriteLine(" [1] Initial Screen | [2] Exhibition Setup | [3] Playoff Bracket");

        Console.Write("Select screen number: ");
        string screenInput = Console.ReadKey(true).KeyChar.ToString();

        Console.Write("\nEnter the NUMBER of the image file to assign: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= imageFiles.Count)
        {
            string selectedFile = "Team/" + imageFiles[index - 1];
            string screenName = screenInput switch {
                "1" => "Initial Screen",
                "2" => "Exhibition Setup",
                "3" => "Playoff Bracket",
                _ => "Unknown Screen"
            };
            
            Console.WriteLine($"\n[SUCCESS] Assigned {selectedFile} as Background for {screenName}. (Global settings update placeholder)");
        }
        else
        {
            Console.WriteLine("\nInvalid file number selected.");
        }
    }
    
    // --- File Utility Methods ---
    private List<string> GetAudioFiles()
    {
        // FIX: Ensure the Audio directory exists before attempting to list files
        if (!Directory.Exists("Audio"))
        {
            try { Directory.CreateDirectory("Audio"); } catch { }
            return new List<string>();
        }
        return Directory.GetFiles("Audio")
                        .Select(Path.GetFileName)
                        .Where(f => f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || 
                                    f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                        .ToList();
    }

    private List<string> GetImageFiles()
    {
        // FIX: Ensure the Team directory exists before attempting to list files
        if (!Directory.Exists("Team"))
        {
            try { Directory.CreateDirectory("Team"); } catch { }
            return new List<string>();
        } 
        return Directory.GetFiles("Team")
                        .Select(Path.GetFileName)
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        .ToList();
    }
}
