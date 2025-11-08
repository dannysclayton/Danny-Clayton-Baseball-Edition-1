using YourSimProject.Models;

public class SystemConfig
{
    public bool EnableSoundEffects { get; set; } = true;
    public bool EnableTeamLogos { get; set; } = true;
    public string DefaultSaveLocation { get; set; } = "SavedSeasons";

    // Preferred export format for team/stat workbooks
    public ExportFormat WorkbookExportFormat { get; set; } = ExportFormat.Xlsx;

    // Placeholder for future UI themes, etc.
}
