
using System;
using System.Media;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GameEngine
{
    // Collection of all teams in the game
    public List<Team> Teams { get; set; } = new List<Team>();

    /// <summary>
    /// Finds a team by its name (case-insensitive).
    /// </summary>
    public Team? FindTeamByName(string name)
    {
        return Teams.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    // Define Constants for Screen/Base Names
    public const string SCREEN_MAIN_MENU = "Main Menu";
    public const string SCREEN_SEASON = "1st Base - Season";
    public const string SCREEN_UPLOAD = "2nd Base - Upload";
    public const string SCREEN_EXHIBITION = "3rd Base - Exhibition Game";
    public const string SCREEN_SETTINGS = "Home Plate - Settings";
    public const string SCREEN_LOAD_SAVE = "Pitcher's Mound - Load Save";

    // Audio file paths (Create an 'Audio' folder and place files there)
    private const string CLICK_SOUND_PATH = "Audio/click.wav";
    private const string PLAY_BALL_SOUND_PATH = "Audio/playball.wav";

    public string CurrentScreen { get; private set; } = SCREEN_MAIN_MENU;

    public void PlayClickSound()
    {
        PlaySound(CLICK_SOUND_PATH);
    }

    private void PlaySound(string path)
    {
        if (File.Exists(path) && OperatingSystem.IsWindows())
        {
            try
            {
                using (var player = new SoundPlayer(path))
                {
                    player.PlaySync();
                }
            }
            catch (Exception)
            {
                // Suppress console errors for missing audio files
            }
        }
    }

    /// <summary>
    /// Navigates to a new screen, playing the required audio.
    /// </summary>
    public void NavigateTo(string screenName)
    {
        if (screenName == SCREEN_LOAD_SAVE)
        {
            PlaySound(PLAY_BALL_SOUND_PATH); // Play 'play ball' for pitcher's mound
        }
        else if (screenName != SCREEN_MAIN_MENU)
        {
            PlayClickSound(); // Play click sound for all base navigation
        }

        CurrentScreen = screenName;
        Console.Clear();
    }
}
