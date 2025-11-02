public class AudioConfig
{
    // Path to the 'click.wav' file played on menu selections
    public string MenuClickSoundPath { get; set; } = "Audio/click.wav"; 
    
    // Path to the 'playball.wav' file played when loading a game
    public string PlayBallSoundPath { get; set; } = "Audio/playball.wav"; 
    
    // Game event sounds
    public string StrikeoutSoundPath { get; set; } = "Audio/strikeout.wav";
    public string HomeRunSoundPath { get; set; } = "Audio/homerun.wav";
    public string UmpireCallPath { get; set; } = "Audio/out.wav";
}
