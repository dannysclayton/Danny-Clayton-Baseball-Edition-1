using System.Collections.Generic;
using System.IO;
using YourSimProject.Models;

namespace YourSimProject.Services
{
public class SeasonManager
{
    private readonly ExcelBuilder _excelBuilder = new();

    /// <summary>
    /// Saves all team statistics to formatted Excel sheets.
    /// This should be called whenever the season state is saved.
    /// </summary>
    public void SaveSeasonStats(
        Team team,
        List<PlayerBattingStats> batters,
        List<PlayerPitchingStats> pitchers,
        List<PlayerFieldingStats> fielders)
    {
        // Example path to the team logo - will pull from your XML metadata structure
        string logoPath = team.LogoPath; 

        // Generate the Excel file
        // Use current global writer factory (can be switched via settings)
        _excelBuilder.GenerateTeamStatSheet(
            team,
            batters,
            pitchers,
            fielders,
            logoPath,
            writer: null);
        
        // You would typically include logging or a message here:
        // Console.WriteLine($"Generated stats sheet for {team.Name}");
    }
}
}