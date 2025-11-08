using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace YourSimProject.Services
{
    public class SeasonDataService
    {
    private const string SAVE_DIRECTORY = "SavedSeasons";
    private const string FILE_EXTENSION = ".json";

    public SeasonDataService()
    {
        // Ensure the directory exists when the service is instantiated
        if (!Directory.Exists(SAVE_DIRECTORY))
        {
            Directory.CreateDirectory(SAVE_DIRECTORY);
        }
    }

    /// <summary>
    /// Returns the list of saved season file names (without extension) in the save directory.
    /// </summary>
    public List<string> GetSavedSeasons()
    {
        if (!Directory.Exists(SAVE_DIRECTORY))
        {
            return new List<string>();
        }

        return Directory
            .EnumerateFiles(SAVE_DIRECTORY, $"*{FILE_EXTENSION}")
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .OrderBy(name => name)
            .ToList();
    }

    /// <summary>
    /// Saves the SeasonState object to a file using JSON serialization.
    /// </summary>
    /// <param name="state">The SeasonState object to save.</param>
    /// <param name="fileName">The user-defined name for the save file.</param>
    public bool SaveSeason(SeasonState state, string fileName)
    {
        try
        {
            // Sanitize the file name to prevent path traversal issues
            string safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            state.SeasonName = safeFileName; // Ensure the state knows its own name

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(state, options);

            string filePath = Path.Combine(SAVE_DIRECTORY, safeFileName + FILE_EXTENSION);
            File.WriteAllText(filePath, jsonString);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to save season: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads a SeasonState object from a specified file.
    /// </summary>
    /// <param name="fileName">The file name (without extension) of the season to load.</param>
    /// <returns>The loaded SeasonState object or null on failure.</returns>
    public SeasonState? LoadSeason(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SAVE_DIRECTORY, fileName + FILE_EXTENSION);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"\n[ERROR] Save file not found: {fileName}");
                return null;
            }

            string jsonString = File.ReadAllText(filePath);
            SeasonState? state = JsonSerializer.Deserialize<SeasonState>(jsonString);
            return state;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to load season: {ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// Deletes a specified season file.
    /// </summary>
    public bool DeleteSeasonFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SAVE_DIRECTORY, fileName + FILE_EXTENSION);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to delete season: {ex.Message}");
            return false;
        }
    }
}
}