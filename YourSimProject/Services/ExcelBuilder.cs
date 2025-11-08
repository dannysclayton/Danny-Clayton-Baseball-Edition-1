using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;
using YourSimProject.Models;
// Reporting abstraction added below (IWorkbookWriter + ClosedXmlWorkbookWriter)

// EPPlus migration complete; removed license context comments.
namespace YourSimProject.Services
{
public class ExcelBuilder
{
    private const string STATS_FOLDER = "Stats";
    // Global default writer factory, can be switched at runtime (CLI/settings)
    public static Func<IWorkbookWriter> DefaultWriterFactory { get; set; } = () => new ClosedXmlWorkbookWriter();

    public ExcelBuilder()
    {
        // Ensure the output directory exists
        if (!Directory.Exists(STATS_FOLDER))
        {
            Directory.CreateDirectory(STATS_FOLDER);
        }
    }
    // ClosedXML requires no license context setup.
    /// <summary>
    /// Generates a complete Excel statistical sheet for a single team.
    /// </summary>
    public void GenerateTeamStatSheet(
        Team team,
        List<PlayerBattingStats> battingStats,
        List<PlayerPitchingStats> pitchingStats,
        List<PlayerFieldingStats> fieldingStats,
    string logoPath,
    IWorkbookWriter? writer = null)
    {
        // Use a clean file name based on the team name
        string safeTeamName = team.Name.Replace(" ", "");
        string fileName = Path.Combine(STATS_FOLDER, $"{safeTeamName}_SeasonStats.xlsx");

    // Choose writer (default to runtime-selected implementation)
    IWorkbookWriter w = writer ?? DefaultWriterFactory();

        // 1. Add Team Summary Tab
        w.AddTeamSummary(team, logoPath);

        // 2. Add Stat Tabs
        w.AddStatSheet("Batting Stats", battingStats);
        w.AddStatSheet("Pitching Stats", pitchingStats);
        w.AddStatSheet("Fielding Stats", fieldingStats);

        // 3. Add Placeholder Tabs
        w.AddPlaceholderSheet("Individual Leaders");
        w.AddPlaceholderSheet("Career Stats");
        w.AddPlaceholderSheet("Historical Records");

        // Save the file
        w.Save(fileName);
    }

    #region Tab Generation Methods

    private void AddTeamSummaryTab(XLWorkbook workbook, Team team, string logoPath)
    {
        var ws = workbook.Worksheets.Add("Team Summary");

        // Apply styling: Black background, white text (sheet-wide)
        StyleSheet(ws);

        // Insert Logo (Top-Left) — optional for ClosedXML; skip if unsupported
        if (File.Exists(logoPath))
        {
            // ClosedXML supports images via AddPicture; API may vary by version.
            // We'll attempt to place it near A1; failures will be swallowed.
            try
            {
                var picture = ws.AddPicture(logoPath)
                                 .MoveTo(ws.Cell(1, 6))
                                 .WithSize(150, 150);
            }
            catch { /* ignore image insertion issues */ }
        }

        // Add Summary Data (Starting below logo space)
        int startRow = 8;
        ws.Cell(startRow++, 1).Value = $"Team: {team.Name}";
        ws.Cell(startRow++, 1).Value = "--- Season Summary ---";
        ws.Cell(startRow++, 1).Value = $"Overall Record: {team.Wins}-{team.Losses}";
        ws.Cell(startRow++, 1).Value = $"Region Record: {team.RegionWins}-{team.RegionLosses}";
        ws.Cell(startRow++, 1).Value = $"Games Behind: {team.GamesBehind}";
        ws.Cell(startRow++, 1).Value = $"Runs Scored: {team.RunsScored}";
        ws.Cell(startRow++, 1).Value = $"Runs Allowed: {team.RunsAllowed}";

        // Apply bolding to the team header
        ws.Cell(8, 1).Style.Font.SetBold();
    }

    private void AddStatTab<T>(XLWorkbook workbook, string tabName, List<T> stats) where T : class
    {
        var ws = workbook.Worksheets.Add(tabName);

        // Insert table from the collection with headers
        var table = ws.Cell(1, 1).InsertTable(stats, tabName, true);

        // Apply styling
        StyleSheet(ws);

    // Make header row bold via table header styling
    table.HeadersRow().Style.Font.SetBold();

        // Autofit columns for readability
        ws.Columns().AdjustToContents();
    }

    private void AddPlaceholderTab(XLWorkbook workbook, string tabName)
    {
        var ws = workbook.Worksheets.Add(tabName);
        StyleSheet(ws);
        ws.Cell(1, 1).Value = $"Data for {tabName} will be generated here.";
    }

    #endregion

    #region Styling Helper

    private void StyleSheet(IXLWorksheet ws)
    {
        // Apply black background and white font for the used range progressively as content is added
        ws.Style.Fill.SetBackgroundColor(XLColor.Black);
        ws.Style.Font.SetFontColor(XLColor.White);
        ws.Style.Font.SetFontName("Calibri");
        ws.Style.Font.SetFontSize(11);
    }

    // You would add a method here later to ApplyHyperlinkStyle(ExcelRangeBase cell, string link)
    // to achieve the blue hyperlink text color for teams.

    #endregion
}
}
namespace YourSimProject.Services
{
    /// <summary>
    /// Small abstraction for writing workbooks; enables future swap (e.g., CSV, other libs) and unit testing.
    /// </summary>
    public interface IWorkbookWriter
    {
        void AddTeamSummary(Team team, string logoPath);
        void AddStatSheet<T>(string name, List<T> rows) where T : class;
        void AddPlaceholderSheet(string name);
        void Save(string filePath);
    }

    /// <summary>
    /// ClosedXML implementation of IWorkbookWriter.
    /// </summary>
    public class ClosedXmlWorkbookWriter : IWorkbookWriter
    {
        private readonly XLWorkbook _workbook = new XLWorkbook();

        public void AddTeamSummary(Team team, string logoPath)
        {
            var ws = _workbook.Worksheets.Add("Team Summary" + (team.Name.Length > 0 ? "" : ""));
            Style(ws);
            if (File.Exists(logoPath))
            {
                try { ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 6)).WithSize(150, 150); } catch { }
            }
            int r = 8;
            ws.Cell(r++,1).Value = $"Team: {team.Name}";
            ws.Cell(r++,1).Value = "--- Season Summary ---";
            ws.Cell(r++,1).Value = $"Overall Record: {team.Wins}-{team.Losses}";
            ws.Cell(r++,1).Value = $"Region Record: {team.RegionWins}-{team.RegionLosses}";
            ws.Cell(r++,1).Value = $"Games Behind: {team.GamesBehind}";
            ws.Cell(r++,1).Value = $"Runs Scored: {team.RunsScored}";
            ws.Cell(r++,1).Value = $"Runs Allowed: {team.RunsAllowed}";
            ws.Cell(8,1).Style.Font.SetBold();
        }

        public void AddStatSheet<T>(string name, List<T> rows) where T: class
        {
            var ws = _workbook.Worksheets.Add(name);
            var table = ws.Cell(1,1).InsertTable(rows, name, true);
            Style(ws);
            table.HeadersRow().Style.Font.SetBold();
            ws.Columns().AdjustToContents();
        }

        public void AddPlaceholderSheet(string name)
        {
            var ws = _workbook.Worksheets.Add(name);
            Style(ws);
            ws.Cell(1,1).Value = $"Data for {name} will be generated here.";
        }

        public void Save(string filePath) => _workbook.SaveAs(filePath);

        private static void Style(IXLWorksheet ws)
        {
            ws.Style.Fill.SetBackgroundColor(XLColor.Black);
            ws.Style.Font.SetFontColor(XLColor.White);
            ws.Style.Font.SetFontName("Calibri");
            ws.Style.Font.SetFontSize(11);
        }
    }

    /// <summary>
    /// CSV implementation of IWorkbookWriter. Each sheet becomes a CSV file alongside the target path.
    /// 'filePath' argument determines the base name and folder; sheet name is appended.
    /// </summary>
    public class CsvWorkbookWriter : IWorkbookWriter
    {
        private readonly List<(string Name, List<string[]> Rows)> _sheets = new();

        public void AddTeamSummary(Team team, string logoPath)
        {
            var rows = new List<string[]> {
                new [] { $"Team: {team.Name}" },
                new [] { "--- Season Summary ---" },
                new [] { $"Overall Record: {team.Wins}-{team.Losses}" },
                new [] { $"Region Record: {team.RegionWins}-{team.RegionLosses}" },
                new [] { $"Games Behind: {team.GamesBehind}" },
                new [] { $"Runs Scored: {team.RunsScored}" },
                new [] { $"Runs Allowed: {team.RunsAllowed}" }
            };
            _sheets.Add(("Team Summary", rows));
        }

        public void AddStatSheet<T>(string name, List<T> rows) where T : class
        {
            var list = new List<string[]>();
            var props = typeof(T).GetProperties();
            list.Add(props.Select(p => p.Name).ToArray());
            foreach (var item in rows)
            {
                list.Add(props.Select(p => (p.GetValue(item)?.ToString()) ?? string.Empty).ToArray());
            }
            _sheets.Add((name, list));
        }

        public void AddPlaceholderSheet(string name)
        {
            _sheets.Add((name, new List<string[]>{ new []{ $"Data for {name} will be generated here." } }));
        }

        public void Save(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath) ?? ".";
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            Directory.CreateDirectory(dir);
            foreach (var (Name, Rows) in _sheets)
            {
                var safe = string.Join("_", Name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                var target = Path.Combine(dir, $"{baseName}_{safe}.csv");
                using var sw = new StreamWriter(target);
                foreach (var r in Rows)
                {
                    sw.WriteLine(string.Join(",", r.Select(EscapeCsv)));
                }
            }
        }

        private static string EscapeCsv(string s)
        {
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            {
                return '"' + s.Replace("\"", "\"\"") + '"';
            }
            return s;
        }
    }
}