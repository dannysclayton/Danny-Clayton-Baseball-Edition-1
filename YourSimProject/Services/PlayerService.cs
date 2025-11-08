using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YourSimProject.Models;

namespace YourSimProject.Services
{
    // NOTE: This service is the placeholder for all player assignment logic.
    public class PlayerService
    {
        public bool AssignPlayersToTeam(string filePath, string teamName, TeamDatabase teamDatabase)
        {
            // 1. Check if the file exists (basic integrity check)
            if (!File.Exists(filePath))
            {
                return false;
            }
            // 2. Find the target team object in the live database
            Team? targetTeam = teamDatabase.GetTeam(teamName);
            if (targetTeam == null)
            {
                return false; 
            }
            // 3. --- Placeholder for Player Data Parsing ---
            // Simulate creating 30 new players (minimum roster size, 1-6 rating range)
            targetTeam.Roster.Clear(); // Clear old roster
            Random rand = new Random();
            for (int i = 1; i <= 30; i++)
            {
                targetTeam.Roster.Add(new Player
                {
                    // FIX: Assigns a unique name and links it to the team name for persistence.
                    Name = $"Upload Player {i} ({teamName})", 
                    ClassLevel = (i % 4 == 0) ? "Senior" : "Freshman",
                    BattingRating = rand.Next(1, 7), // 1 to 6
                    PitchingRating = rand.Next(1, 7), // 1 to 6
                    Positions = new List<string> { (i % 2 == 0) ? "P" : "SS", (i % 3 == 0) ? "C" : "None" },
                    // FIX: Assign the team name to the player object for persistence
                    TeamName = targetTeam.Name 
                });
            }
            return true;
        }
    }
}
