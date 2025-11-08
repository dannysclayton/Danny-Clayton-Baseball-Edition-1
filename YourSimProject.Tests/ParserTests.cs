using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using YourSimProject.Services;
using Xunit;

public class ParserTests
{
    [Fact]
    public void ParseStructure_Csv_ReturnsConferencesWithRegionsAndTeams()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), "structure_test_unit.csv");
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

        conferences.Should().HaveCount(2);
        conferences[0].Regions.Should().HaveCount(2);
        conferences[0].Regions.Sum(r => r.Teams.Count).Should().Be(3);
        conferences[1].Regions.Should().HaveCount(1);
        conferences[1].Regions.Sum(r => r.Teams.Count).Should().Be(1);
    }
}
