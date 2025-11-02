using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

// Set the EPPlus license context
// NOTE: EPPlus requires this line for non-commercial use since version 5.
public class ExcelBuilder
{
    private const string STATS_FOLDER = "Stats";

    public ExcelBuilder()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        // Ensure the output directory exists
        if (!Directory.Exists(STATS_FOLDER))
        {
            Directory.CreateDirectory(STATS_FOLDER);
        }
    }

    /// <summary>
    /// Generates a complete Excel statistical sheet for a single team.
    /// </summary>
    public void GenerateTeamStatSheet(
        Team team,
        List<PlayerBattingStats> battingStats,
        List<PlayerPitchingStats> pitchingStats,
        List<PlayerFieldingStats> fieldingStats,
        string logoPath)
    {
        // Use a clean file name based on the team name
        string safeTeamName = team.Name.Replace(" ", "");
        string fileName = Path.Combine(STATS_FOLDER, $"{safeTeamName}_SeasonStats.xlsx");

        using var package = new ExcelPackage();
        
        // 1. Add Team Summary Tab
        AddTeamSummaryTab(package, team, logoPath);

        // 2. Add Stat Tabs (using LoadFromCollection to populate)
        AddStatTab(package, "Batting Stats", battingStats);
        AddStatTab(package, "Pitching Stats", pitchingStats);
        AddStatTab(package, "Fielding Stats", fieldingStats);
        
        // 3. Add Placeholder Tabs (Logic for data population to be added later)
        AddPlaceholderTab(package, "Individual Leaders");
        AddPlaceholderTab(package, "Career Stats");
        AddPlaceholderTab(package, "Historical Records");

        // Save the file
        package.SaveAs(new FileInfo(fileName));
    }

    #region Tab Generation Methods

    private void AddTeamSummaryTab(ExcelPackage package, Team team, string logoPath)
    {
        var ws = package.Workbook.Worksheets.Add("Team Summary");
        
        // Apply styling: Black background, white text
        StyleSheet(ws);

        // Insert Logo (Top-Left)
        if (File.Exists(logoPath))
        {
            var logo = ws.Drawings.AddPicture("Logo", new FileInfo(logoPath));
            // Position the logo (Row 0, Col 0, Pixel Offset Y, Pixel Offset X)
            logo.SetPosition(0, 0, 5, 5); 
            logo.SetSize(150, 150);
        }

        // Add Summary Data (Starting below logo space)
        int startRow = 8;
        ws.Cells[startRow++, 1].Value = $"Team: {team.Name}";
        ws.Cells[startRow++, 1].Value = "--- Season Summary ---";
        ws.Cells[startRow++, 1].Value = $"Overall Record: {team.Wins}-{team.Losses}";
        ws.Cells[startRow++, 1].Value = $"Region Record: {team.RegionWins}-{team.RegionLosses}";
        ws.Cells[startRow++, 1].Value = $"Games Behind: {team.GamesBehind}";
        ws.Cells[startRow++, 1].Value = $"Runs Scored: {team.RunsScored}";
        ws.Cells[startRow++, 1].Value = $"Runs Allowed: {team.RunsAllowed}";
        
        // Apply bolding and hyperlink style to the team name (if needed for later dashboards)
        ws.Cells[8, 1].Style.Font.Bold = true;
    }

    private void AddStatTab<T>(ExcelPackage package, string tabName, List<T> stats) where T : class
    {
        var ws = package.Workbook.Worksheets.Add(tabName);
        
        // Load data into the worksheet
        // 'true' means include headers from model properties
        ws.Cells["A1"].LoadFromCollection(stats, true);
        
        // Apply styling
        StyleSheet(ws);
        
        // Make the headers bold
        ws.Cells[1, 1, 1, ws.Dimension.End.Column].Style.Font.Bold = true;
        
        // Autofit columns for readability
        ws.Cells[ws.Dimension.Address].AutoFitColumns();
    }

    private void AddPlaceholderTab(ExcelPackage package, string tabName)
    {
        var ws = package.Workbook.Worksheets.Add(tabName);
        StyleSheet(ws);
        ws.Cells["A1"].Value = $"Data for {tabName} will be generated here.";
    }

    #endregion

    #region Styling Helper

    private void StyleSheet(ExcelWorksheet ws)
    {
        // Apply Black Background to all cells
        ws.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells.Style.Fill.BackgroundColor.SetColor(Color.Black);

        // Apply White Text to all cells
        ws.Cells.Style.Font.Color.SetColor(Color.White);

        // Set default font (optional, but good practice)
        ws.Cells.Style.Font.Name = "Calibri";
        ws.Cells.Style.Font.Size = 11;
    }

    // You would add a method here later to ApplyHyperlinkStyle(ExcelRangeBase cell, string link)
    // to achieve the blue hyperlink text color for teams.

    #endregion
}