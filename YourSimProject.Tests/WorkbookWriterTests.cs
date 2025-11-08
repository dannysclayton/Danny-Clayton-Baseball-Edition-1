using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using YourSimProject.Models;
using YourSimProject.Services;
using Xunit;

public class WorkbookWriterTests
{
    private static Team MakeTeam() => new Team {
        Name = "UnitTeam",
        RegionName = "UnitRegion",
        ConferenceName = "UnitConf",
        Coach = new Coach { Name = "Coach", Level = "Average", Style = "Run of the Mill", Experience = 0 },
        LogoPath = "", UniformPath = "", TeamPhotoPath = "", TeamFieldingPhotoPath = "", TeamBattingPhotoPath = "", TeamPitchingPhotoPath = ""
    };

    [Fact]
    public void CsvWorkbookWriter_WritesCsvFiles()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "ysp_csv_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        var target = Path.Combine(tmpDir, "out.xlsx"); // base name only

        var writer = new CsvWorkbookWriter();
        writer.AddTeamSummary(MakeTeam(), "");
        writer.AddStatSheet("Batting Stats", new List<PlayerBattingStats>{ new PlayerBattingStats { PlayerName = "Batter", G = 1, AB = 4, H = 2 } });
        writer.AddPlaceholderSheet("Leaders");
        writer.Save(target);

        Directory.GetFiles(tmpDir, "out_*.csv").Should().NotBeEmpty();
    }
}
