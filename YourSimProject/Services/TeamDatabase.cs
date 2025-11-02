using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.Json;

// The 'partial' keyword is essential if parts of this class exist in other files.
public partial class TeamDatabase
{
    // FIX: Ensure public set access for JSON deserialization of complex objects.
    public List<Conference> Conferences { get; set; } = new();
    
    // FIX: Ensure public set access for JSON deserialization of complex objects.
    public List<Coach> Coaches { get; set; } = new(); 

    private const string DATABASE_FILE_PATH = "league_data.json";


    // --- Core Structure Management ---

    public void LoadStructure(string filePath)
    {
        var parser = new ExcelStructureParser();
        Conferences = parser.ParseStructure(filePath);
    }

    public Team GetTeam(string name)
    {
        // Use StringComparison.OrdinalIgnoreCase for robust lookup regardless of case
        return Conferences
            .SelectMany(c => c.Regions)
            .SelectMany(r => r.Teams)
            .FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    // --- DATABASE PERSISTENCE METHODS ---

    /// <summary>
    /// Saves the entire database (Conferences, Regions, Teams, Coaches) to a single JSON file.
    /// </summary>
    public bool SaveDatabase()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            // Serialize the whole TeamDatabase object
            string jsonString = JsonSerializer.Serialize(this, options); 
            File.WriteAllText(DATABASE_FILE_PATH, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to save database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads the entire database from a JSON file.
    /// </summary>
    public bool LoadDatabase()
    {
        try
            {
            if (!File.Exists(DATABASE_FILE_PATH))
            {
                return false;
            }

            string jsonString = File.ReadAllText(DATABASE_FILE_PATH);
            var loadedDb = JsonSerializer.Deserialize<TeamDatabase>(jsonString);

            if (loadedDb != null)
            {
                // Overwrite the current lists with the loaded data
                // This ensures the live data lists (Conferences, Coaches) are updated by reference.
                this.Conferences = loadedDb.Conferences;
                this.Coaches = loadedDb.Coaches;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to load database: {ex.Message}");
            return false;
        }
    }

    // --- Helper Methods Used by UI ---
    public IEnumerable<string> GetConferenceNames() => Conferences.Select(c => c.Name);

    public List<Player> GetAllPlayers()
    {
        // This gathers all players from all teams across the entire structure
        return Conferences
            .SelectMany(c => c.Regions)
            .SelectMany(r => r.Teams)
            // Ensure Roster list is initialized before attempting to access
            .Where(t => t.Roster != null) 
            .SelectMany(t => t.Roster)
            .ToList();
    }
    
    public List<Coach> GetAllCoaches() 
    {
        // Method required by SettingsScreen.cs to list coaches
        return Coaches;
    }
}
