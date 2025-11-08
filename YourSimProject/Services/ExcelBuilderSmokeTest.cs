using System;
using System.Collections.Generic;
using YourSimProject.Models;
using YourSimProject.Services;

// Minimal smoke test for ExcelBuilder and ClosedXmlWorkbookWriter.
public static class ExcelBuilderSmokeTest
{
    public static void Run()
    {
        var team = new Team {
            Name = "SmokeTesters",
            RegionName = "Test Region",
            ConferenceName = "Test Conference",
            Coach = new Coach { Name = "Test Coach", Level = "Average", Style = "Run of the Mill", Experience = 0 },
            LogoPath = "", UniformPath = "", TeamPhotoPath = "", TeamFieldingPhotoPath = "", TeamBattingPhotoPath = "", TeamPitchingPhotoPath = ""
        };

    var batting = new List<PlayerBattingStats>{ new PlayerBattingStats { PlayerName = "Batter A", G = 10, AB = 30, H = 12, HR = 2, RBI = 7, SO = 5, BB = 4 } };
    var pitching = new List<PlayerPitchingStats>{ new PlayerPitchingStats { PlayerName = "Pitcher A", G = 5, IP = 20.2, SO = 18, BB = 5, ER = 6, H = 15, W = 2, L = 1 } };
    var fielding = new List<PlayerFieldingStats>{ new PlayerFieldingStats { PlayerName = "Fielder A", G = 10, PO = 20, A = 3, E = 2 } };

        var builder = new ExcelBuilder();
        builder.GenerateTeamStatSheet(team, batting, pitching, fielding, logoPath: "");
        Console.WriteLine("[BUILDER TEST] Generated workbook in Stats folder.");
    }
}
