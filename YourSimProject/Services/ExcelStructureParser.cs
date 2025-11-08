using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel; // Using ClosedXML for XLSX parsing
using YourSimProject.Models;

namespace YourSimProject.Services
{
public class ExcelStructureParser
{
    // ClosedXML requires no license context setup for typical usage.

    /// <summary>
    /// Parses the league structure from an XLSX or a text-based CSV file.
    /// </summary>
    public List<Conference> ParseStructure(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found. Ensure '{Path.GetFileName(filePath)}' is in the project's main folder.", filePath);
        }

        var extension = Path.GetExtension(filePath)?.ToLower();
        List<string> structureLines;

        if (extension == ".xlsx")
        {
            // --- LOGIC 1: ClosedXML for XLSX files ---
            structureLines = ReadXlsxFile(filePath);
        }
        else if (extension == ".csv" || extension == ".txt")
        {
            // --- LOGIC 2: Standard C# for CSV/Text files (Plain text format) ---
            structureLines = File.ReadAllLines(filePath).ToList();
        }
        else
        {
            throw new FormatException($"Unsupported file format '{extension}'. Please use .xlsx, .csv, or .txt.");
        }

        if (structureLines == null || structureLines.Count == 0)
        {
            throw new InvalidDataException("The file was read successfully but contains no data.");
        }

        return ProcessStructureLines(structureLines);
    }
    
    /// <summary>
    /// Uses ClosedXML to extract cell values from the first column of the first sheet.
    /// </summary>
    private List<string> ReadXlsxFile(string filePath)
    {
        var lines = new List<string>();
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheets.First();
            foreach (var row in sheet.RowsUsed())
            {
                var cellValue = row.Cell(1).GetString();
                if (!string.IsNullOrWhiteSpace(cellValue))
                {
                    lines.Add(cellValue);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to read Excel file '{Path.GetFileName(filePath)}'. Details: {ex.Message}");
        }
        return lines;
    }

    /// <summary>
    /// Processes lines into Conference, Region, and Team objects.
    /// This logic is shared between XLSX and CSV/TXT readers.
    /// </summary>
    private List<Conference> ProcessStructureLines(List<string> lines)
    {
    var conferences = new List<Conference>();
    Conference? currentConference = null;
    Region? currentRegion = null;

        foreach (var line in lines)
        {
            // Use the full line/cell value for processing
            var cellValue = line.Split(',')[0].Trim(); 
            
            if (string.IsNullOrWhiteSpace(cellValue)) continue;

            if (cellValue.EndsWith("Conference", StringComparison.OrdinalIgnoreCase))
            {
                // Found a new Conference header
                currentConference = new Conference { Name = cellValue };
                conferences.Add(currentConference);
                currentRegion = null; // Reset region when conference changes
            }
            else if (cellValue.Contains("Region"))
            {
                // Found a new Region header
                currentRegion = new Region
                {
                    Name = cellValue,
                    ConferenceName = currentConference?.Name ?? "Unknown"
                };
                currentConference?.Regions.Add(currentRegion);
            }
            else if (currentRegion != null)
            {
                // Found a Team within the current Region
                var team = new Team
                {
                    Name = cellValue,
                    RegionName = currentRegion.Name,
                    ConferenceName = currentConference?.Name ?? "Unknown",
                    Coach = new Coach { Name = "Default", Level = "Average", Style = "Run of the Mill", Experience = 0 },
                    LogoPath = "",
                    UniformPath = "",
                    TeamPhotoPath = "",
                    TeamFieldingPhotoPath = "",
                    TeamBattingPhotoPath = "",
                    TeamPitchingPhotoPath = ""
                };
                currentRegion.Teams.Add(team);
            }
        }
        
        if (conferences.Count == 0)
        {
            throw new FormatException($"File contains no valid Conference definitions. Please ensure headers end with 'Conference'.");
        }
        
        return conferences;
    }
}
}
