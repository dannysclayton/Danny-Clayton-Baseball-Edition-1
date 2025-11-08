using System.Collections.Generic;

namespace YourSimProject.Models
{
    public class Conference
    {
        public required string Name { get; set; }
        public List<Region> Regions { get; set; } = new();
    }

    public class Region
    {
        public required string Name { get; set; }
        public required string ConferenceName { get; set; }
        public List<Team> Teams { get; set; } = new();
    }
}
