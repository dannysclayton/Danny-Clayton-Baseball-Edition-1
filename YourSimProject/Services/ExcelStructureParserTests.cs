using System;
using System.IO;
using System.Linq;
using YourSimProject.Services;

// Minimal ad-hoc test harness for ExcelStructureParser using CSV fallback (avoids needing real XLSX in this smoke test)
// This is NOT a full unit test framework; it simply writes results to console when invoked manually.
public static class ExcelStructureParserTests
{
    public static void Run()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), "structure_test.csv");
        File.WriteAllLines(tempFile, new []{
            "Alpha Conference",
            "North Region",
            "Team One",
            "Team Two",
            "South Region",
            "Team Three",
            "Beta Conference",
            "Central Region",
            "Team Four"
        });

        var parser = new ExcelStructureParser();
        var conferences = parser.ParseStructure(tempFile);
        Console.WriteLine($"[PARSER TEST] Conferences: {conferences.Count}");
        foreach (var c in conferences)
        {
            Console.WriteLine($"  Conference: {c.Name} Regions={c.Regions.Count} Teams={c.Regions.Sum(r => r.Teams.Count)}");
        }
    }
}
