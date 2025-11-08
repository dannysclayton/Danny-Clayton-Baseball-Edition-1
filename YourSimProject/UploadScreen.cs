using YourSimProject.Models;
using YourSimProject.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// NOTE: PlayerService is now in its own file: Services/PlayerService.cs

namespace YourSimProject
{
    public class UploadScreen
    {
        // Stub for missing method to fix build error
        private void HandleDataFileAssignment(string assetType, string filePath)
        {
            Console.WriteLine($"[STUB] HandleDataFileAssignment called for {assetType} with file {filePath}.");
            // TODO: Implement actual logic for assigning data files to assets
        }
        private readonly GameEngine _engine;
        private readonly TeamDatabase _teamDatabase; 
        private readonly PlayerService _playerService; 

        public UploadScreen(GameEngine engine, TeamDatabase teamDatabase)
        {
            _engine = engine;
            _teamDatabase = teamDatabase;
            // The service must be initialized before use
            _playerService = new PlayerService(); 
        }

        public void DisplayAndHandle()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- 2ND BASE: UPLOAD ASSETS ---");
                Console.WriteLine($"--- Current League Status: {_teamDatabase.Conferences.Sum(c => c.Regions.Count)} Regions Loaded ---");

                Console.WriteLine("\n--- DATA ASSET UPLOAD (.XML, .JSON, .CSV) ---");
                Console.WriteLine(" [1] Upload Conferences/Regions/Teams Structure");
                Console.WriteLine(" [2] Upload Players Data (Assign to Team)");
                Console.WriteLine(" [3] Upload Coaches Data (Assign to Team)");

                Console.WriteLine("\n--- MEDIA ASSET UPLOAD ---");
                Console.WriteLine(" [4] Upload Audio Files (.WAV, .MP3)");
                Console.WriteLine(" [5] Upload Team Images (.JPG, .PNG)");

                Console.WriteLine(" [B] Back to Main Menu");

                Console.Write("Enter selection: ");
                string input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
                _engine.PlayClickSound();
                Console.WriteLine();

                if (input == "B")
                {
                    _engine.NavigateTo(GameEngine.SCREEN_MAIN_MENU);
                    return;
                }

                string filePath = PromptForFilePath();
                if (string.IsNullOrWhiteSpace(filePath)) continue;

                string fileName = Path.GetFileName(filePath);
                Console.WriteLine($"\nFile selected: {fileName}");

                // --- Display the file preview here ---
                DisplayFilePreview(filePath);

                Console.Write($"Confirm upload of {fileName}? (Y/N): ");
                if (Console.ReadKey(true).KeyChar.ToString().ToUpper() != "Y") continue;

                Console.WriteLine();

                // Handle asset-specific logic
                switch (input)
                {
                    case "1":
                    HandleStructureUpload(filePath);
                    break;
                case "2":
                    HandlePlayerAssignmentFlow(filePath); // FIX: Launch player selection flow
                    break;
                case "3":
                    HandleDataFileAssignment("Coaches", filePath);
                    break;
                case "4":
                    HandleMediaFileTransfer(filePath, "Audio", "wav, mp3");
                    break;
                case "5":
                    HandleMediaFileTransfer(filePath, "Team", "jpg, jpeg, png");
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    /// <summary>
    /// --- FIX: NEW PLAYER UPLOAD FLOW WITH SELECTION ---
    /// Reads file, launches selector, and assigns chosen players to a team.
    /// </summary>
    private void HandlePlayerAssignmentFlow(string filePath)
    {
        if (_teamDatabase.Conferences.Count == 0)
        {
            Console.WriteLine("\n[ERROR] Please upload the league structure (Option 1) first.");
            return;
        }

        Console.WriteLine("\n--- ASSIGN PLAYERS DATA ---");

        // 1. Select Target Team
    string? conferenceName = SelectConference();
    if (conferenceName == null) return;
    string? regionName = SelectRegion(conferenceName);
    if (regionName == null) return;
    string? teamName = SelectTeam(conferenceName, regionName);
    if (teamName == null) return;
        
    Team? targetTeam = _teamDatabase.GetTeam(teamName);
    if (targetTeam == null) return; // Safety check

        // 2. Simulate Reading ALL potential players from the file (100 total)
        // In a real app, this would use a dedicated PlayerDataParser.
        List<Player> potentialPlayers = ReadPlayersFromCsv(filePath);
        if (potentialPlayers.Count == 0)
        {
            Console.WriteLine("\n[ERROR] File read error or no players found in file.");
            return;
        }

        // --- Duplicate Name Review ---
        var grouped = potentialPlayers.GroupBy(p => p.Name).Where(g => g.Count() > 1).ToList();
        if (grouped.Count > 0)
        {
            Console.WriteLine("\n[WARNING] Duplicate player names detected. Please review and select which to keep:");
            foreach (var group in grouped)
            {
                Console.WriteLine($"\nPlayer Name: {group.Key}");
                var list = group.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    string positions = string.Join("/", p.Positions.Where(pos => pos != "None"));
                    Console.WriteLine($" [{i + 1}] {p.Name} | {positions} | Bat: {p.BattingRating} | Pit: {p.PitchingRating}");
                }
                Console.Write($"Select which version to keep for '{group.Key}' (enter ID): ");
                int keepIdx = -1;
                while (keepIdx < 1 || keepIdx > list.Count)
                {
                    var input = Console.ReadLine()?.Trim();
                    int.TryParse(input, out keepIdx);
                }
                // Remove all but selected
                for (int i = 0; i < list.Count; i++)
                {
                    if (i != (keepIdx - 1)) potentialPlayers.Remove(list[i]);
                }
            }
            Console.WriteLine("\nDuplicates resolved. Press any key to continue...");
            Console.ReadKey(true);
        }

        // 3. Launch Selection Editor
        HandlePlayerSelectionMenu(targetTeam, potentialPlayers);
    }
    
    private void HandlePlayerSelectionMenu(Team team, List<Player> potentialPlayers)
    {
        List<Player> selectedRoster = new List<Player>();
        HashSet<int> selectedIndices = new HashSet<int>();

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n--- ROSTER SELECTION FOR: {team.Name} ---");
            Console.WriteLine($" Players Selected: {selectedRoster.Count} / 30");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine(" ID | Class | Name                   | Positions   | Bat | Pit | Status");
            Console.WriteLine("-----------------------------------------------------------------");

            for (int i = 0; i < potentialPlayers.Count; i++)
            {
                Player p = potentialPlayers[i];
                string status = selectedIndices.Contains(i) ? "[SELECTED]" : " ";
                string positions = string.Join("/", p.Positions.Where(pos => pos != "None").Take(3));

                Console.WriteLine($" [{i + 1:D2}] | {p.ClassLevel,-9} | {p.Name,-22} | {positions,-11} | {p.BattingRating,-3} | {p.PitchingRating,-3} | {status}");
            }

            Console.WriteLine("\nOptions: [ID] Select/Deselect Player | [A] Assign Final Roster | [B] Back");
            Console.Write($"Enter selection (ID or A/B): ");
            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (input == "B") return; // Back to previous menu

            if (input == "A")
            {
                if (selectedRoster.Count < 9)
                {
                    Console.WriteLine("\n[ERROR] Roster must have at least 9 players assigned before finalizing.");
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                }
                else if (selectedRoster.Count > 30)
                {
                    Console.WriteLine("\n[ERROR] Roster cannot exceed 30 players. Deselect a player first.");
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                }
                else
                {
                    // 4. Finalize Assignment
                    // --- Normalize positions for selected roster ---
                    var infieldSlots = new[] { "1B", "2B", "3B", "SS" };
                    var outfieldSlots = new[] { "LF", "CF", "RF" };
                    var infieldCount = new Dictionary<string, int> { {"1B",0}, {"2B",0}, {"3B",0}, {"SS",0} };
                    var outfieldCount = new Dictionary<string, int> { {"LF",0}, {"CF",0}, {"RF",0} };

                    // Count current assignments
                    foreach (var p in selectedRoster)
                    {
                        foreach (var pos in p.Positions)
                        {
                            if (infieldCount.ContainsKey(pos)) infieldCount[pos]++;
                            if (outfieldCount.ContainsKey(pos)) outfieldCount[pos]++;
                        }
                    }

                    foreach (var p in selectedRoster)
                    {
                        for (int i = 0; i < p.Positions.Count; i++)
                        {
                            string pos = p.Positions[i];
                            // Normalize LHP/RHP to P
                            if (pos == "LHP" || pos == "RHP") p.Positions[i] = "P";
                            // Normalize UTL/Utility to INF
                            else if (pos == "UTL" || pos == "Utility") p.Positions[i] = "INF";
                            // For INF, assign to least filled infield slot
                            else if (pos == "INF")
                            {
                                var least = infieldCount.OrderBy(kv => kv.Value).First().Key;
                                p.Positions[i] = least;
                                infieldCount[least]++;
                            }
                            // For OF, assign to least filled outfield slot
                            else if (pos == "OF")
                            {
                                var least = outfieldCount.OrderBy(kv => kv.Value).First().Key;
                                p.Positions[i] = least;
                                outfieldCount[least]++;
                            }
                        }
                    }

                    team.Roster.Clear();
                    team.Roster.AddRange(selectedRoster);
                    // Ensure TeamName is set for all selected players
                    foreach(var p in team.Roster) { p.TeamName = team.Name; }
                    _teamDatabase.SaveDatabase(); // Persist the roster change
                    Console.WriteLine($"\n[SUCCESS] Roster finalized and {team.Roster.Count} players assigned to {team.Name}.");

                    // Print out the roster for confirmation
                    Console.WriteLine("\n[DEBUG] Current Roster for this team:");
                    for (int i = 0; i < team.Roster.Count; i++)
                    {
                        var p = team.Roster[i];
                        string positions = string.Join("/", p.Positions.Where(pos => pos != "None").Take(3));
                        Console.WriteLine($" [{i + 1:D2}] {p.Name} | {p.ClassLevel} | {positions} | Bat: {p.BattingRating} | Pit: {p.PitchingRating}");
                    }

                    // --- Normalize positions for remaining players ---
                    var remainingPlayers = potentialPlayers.Except(selectedRoster).ToList();
                    if (remainingPlayers.Count > 0)
                    {
                        string teamFolder = Path.Combine("Teams", team.Name);
                        Directory.CreateDirectory(teamFolder);
                        string filePath = Path.Combine(teamFolder, "remaining_players.csv");
                        using (var writer = new StreamWriter(filePath, false))
                        {
                            writer.WriteLine("Player Name,Class,1st Position,2nd Position,3rd Position,Batting Rating,Pitching Rating");
                            foreach (var p in remainingPlayers)
                            {
                                // Normalize positions for remaining players
                                var normPos = new List<string>();
                                for (int i = 0; i < 3; i++)
                                {
                                    string pos = p.Positions.Count > i ? p.Positions[i] : "None";
                                    if (pos == "LHP" || pos == "RHP") pos = "P";
                                    else if (pos == "UTL" || pos == "Utility") pos = "INF";
                                    normPos.Add(pos);
                                }
                                writer.WriteLine($"{p.Name},{p.ClassLevel},{normPos[0]},{normPos[1]},{normPos[2]},{p.BattingRating},{p.PitchingRating}");
                            }
                        }
                        Console.WriteLine($"\n[INFO] Remaining players saved to {filePath}");
                    }

                    // Check for duplicate teams with the same name
                    var allTeams = _teamDatabase.Conferences.SelectMany(c => c.Regions).SelectMany(r => r.Teams).ToList();
                    int duplicateCount = allTeams.Count(t => t.Name.Equals(team.Name, StringComparison.OrdinalIgnoreCase));
                    if (duplicateCount > 1)
                    {
                        Console.WriteLine($"\n[WARNING] There are {duplicateCount} teams named '{team.Name}' in the database. This may cause roster confusion.");
                    }

                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    return;
                }
            }

            if (int.TryParse(input, out int id) && id > 0 && id <= potentialPlayers.Count)
            {
                int index = id - 1;
                if (selectedIndices.Contains(index))
                {
                    selectedIndices.Remove(index);
                    selectedRoster.Remove(potentialPlayers[index]);
                }
                else if (selectedRoster.Count < 30)
                {
                    selectedIndices.Add(index);
                    selectedRoster.Add(potentialPlayers[index]);
                }
                else
                {
                    Console.WriteLine("\n[ALERT] Roster is full (30 players). Deselect a player first.");
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
    }

    /// <summary>
    /// Reads player data from a CSV file for selection.
    /// CSV format: Name,ClassLevel,BattingRating,PitchingRating,Positions (comma-separated)
    /// Example line: John Doe,Senior,5,3,SS/P/C
    private List<Player> ReadPlayersFromCsv(string filePath)
    {
        var players = new List<Player>();
        try
        {
            bool isHeader = true;
            foreach (var line in File.ReadLines(filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue; // Skip empty or comment lines
                var parts = trimmed.Split(',');
                if (isHeader) { isHeader = false; continue; } // Skip header row
                if (parts.Length < 8) continue; // Require all fields

                string name = parts[1].Trim(); // Player Name
                string classLevel = parts[2].Trim(); // Class
                var positions = new List<string>();
                positions.Add(!string.IsNullOrWhiteSpace(parts[3]) ? parts[3].Trim() : "None"); // 1st Position
                positions.Add(!string.IsNullOrWhiteSpace(parts[4]) ? parts[4].Trim() : "None"); // 2nd Position
                positions.Add(!string.IsNullOrWhiteSpace(parts[5]) ? parts[5].Trim() : "None"); // 3rd Position
                int batting = int.TryParse(parts[6].Trim(), out int b) ? b : 1;
                int pitching = int.TryParse(parts[7].Trim(), out int p) ? p : 1;

                players.Add(new Player
                {
                    Name = name,
                    ClassLevel = classLevel,
                    BattingRating = batting,
                    PitchingRating = pitching,
                    Positions = positions.Count > 0 ? positions : new List<string> { "None" }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read CSV: {ex.Message}");
        }
        return players;
    }
    
    
    private long GetFileSize(string filePath)
    {
        try
        {
            return new FileInfo(filePath).Length;
        }
        catch { return 0; }
    }
    
    /// <summary>
    /// Displays the first few lines of text content for data files, or a message for media.
    /// </summary>
    private void DisplayFilePreview(string filePath)
    {
        string extension = (Path.GetExtension(filePath) ?? string.Empty).ToLowerInvariant();

        Console.WriteLine("\n--- FILE PREVIEW ---");

        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".wav" || extension == ".mp3")
        {
            Console.WriteLine($"[MEDIA FILE] Thumbnail/Playback controls would appear here. File size: {GetFileSize(filePath)} bytes.");
            return;
        }

        try
        {
            // Read the first 5 lines for a data file preview
            var lines = File.ReadLines(filePath).Take(5).ToList();
            if (lines.Count == 0)
            {
                Console.WriteLine("[DATA FILE] File is empty.");
                return;
            }

            Console.WriteLine("[DATA STRUCTURE PREVIEW (First 5 Lines):]");
            foreach (var line in lines)
            {
                Console.WriteLine($" > {line}");
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("[ERROR] Cannot read file: File not found. (Use file name or drag/paste full path)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Could not read file content for preview: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Loads the Conference/Region/Team structure from the uploaded file into the database.
    /// </summary>
    private void HandleStructureUpload(string filePath)
    {
        try
        {
            // Note: The parser is designed to read XLSX/CSV data structure
            _teamDatabase.LoadStructure(filePath);
            Console.WriteLine($"\n[SUCCESS] Loaded structure: {_teamDatabase.Conferences.Count} Conferences now active.");
            Console.WriteLine("Teams are now available for assignment in the Editor (Home Plate).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[FATAL ERROR] Failed to parse structure. Ensure file path is correct and file is a valid Excel format. Error: {ex.Message}");
        }
    }

    private string? SelectConference()
    {
        var conferenceNames = _teamDatabase.GetConferenceNames().ToList();
    if (conferenceNames.Count == 0) return null;

        Console.WriteLine("\nSelect Conference:");
        for (int i = 0; i < conferenceNames.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {conferenceNames[i]}");
        }
        Console.Write("Enter conference number: ");
        
        if (int.TryParse(Console.ReadLine(), out int confIndex) && confIndex > 0 && confIndex <= conferenceNames.Count)
        {
            return conferenceNames[confIndex - 1];
        }
    Console.WriteLine("Invalid selection or back requested.");
    return null;
    }

    private string? SelectRegion(string conferenceName)
    {
    var conference = _teamDatabase.Conferences.FirstOrDefault(c => c.Name == conferenceName);
        var regionList = conference?.Regions;

        if (regionList == null || regionList.Count == 0)
        {
            Console.WriteLine($"No regions found in {conferenceName}.");
            return null;
        }

        Console.WriteLine($"\nSelect Region in {conferenceName}:");
        for (int i = 0; i < regionList.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {regionList[i].Name}");
        }
        Console.Write("Enter region number: ");
        
        if (int.TryParse(Console.ReadLine(), out int regIndex) && regIndex > 0 && regIndex <= regionList.Count)
        {
            return regionList[regIndex - 1].Name;
        }
    Console.WriteLine("Invalid selection or back requested.");
    return null;
    }

    private string? SelectTeam(string conferenceName, string regionName)
    {
        var teamList = _teamDatabase.Conferences
            .FirstOrDefault(c => c.Name == conferenceName)?
            .Regions.FirstOrDefault(r => r.Name == regionName)?
            .Teams;

        if (teamList == null || teamList.Count == 0)
        {
            Console.WriteLine($"No teams found in {regionName}.");
            return null;
        }

        Console.WriteLine($"\nSelect Team in {regionName}:");
        for (int i = 0; i < teamList.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {teamList[i].Name}");
        }
        Console.Write("Enter team number: ");
        
        if (int.TryParse(Console.ReadLine(), out int teamIndex) && teamIndex > 0 && teamIndex <= teamList.Count)
        {
            return teamList[teamIndex - 1].Name;
        }
    Console.WriteLine("Invalid selection or back requested.");
    return null;
    }
    
    private string PromptForFilePath()
    {
        Console.WriteLine("\n[Drag & Drop/Paste Path] Place the file in the project directory, or drag it into the console window.");
        Console.Write("Enter file name or paste full path: ");
        
        string inputPath = Console.ReadLine()?.Trim() ?? string.Empty;

        if (inputPath.StartsWith("\"") && inputPath.EndsWith("\""))
        {
            inputPath = inputPath.Substring(1, inputPath.Length - 2);
        }
        
        if (inputPath.Contains(Path.DirectorySeparatorChar) || inputPath.Contains(Path.AltDirectorySeparatorChar))
        {
            return inputPath;
        }
        else
        {
            return inputPath;
        }
    }

    private void HandleMediaFileTransfer(string sourcePath, string destinationFolder, string expectedFormats)
    {
        string fileName = Path.GetFileName(sourcePath);
        string extension = (Path.GetExtension(sourcePath) ?? string.Empty).ToLower().TrimStart('.');

        var validExtensions = expectedFormats
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim()); 

        if (string.IsNullOrWhiteSpace(extension) || !validExtensions.Contains(extension))
        {
            Console.WriteLine($"\n[ERROR] File extension '{extension}' is not valid for {destinationFolder} upload. Expected: {expectedFormats}.");
            return;
        }

    Console.WriteLine($"\n[SUCCESS] Media file {Path.GetFileName(sourcePath)} transferred (conceptually) to /{destinationFolder}/.");
}
    }
}
