using System.Linq;
using YourSimProject; // Screens namespace
using YourSimProject.Services;
using YourSimProject.Models;

namespace YourSimProject.WinForms
{
    public static class AppBootstrap
    {
        public static GameEngine Engine { get; private set; } = new GameEngine();
        public static TeamDatabase TeamDb { get; private set; } = new TeamDatabase();

        public static void Initialize()
        {
            // Load team database similar to SettingsScreen constructor
            TeamDb.LoadDatabase();
            Engine.Teams = TeamDb.Conferences
                .SelectMany(c => c.Regions)
                .SelectMany(r => r.Teams)
                .ToList();
        }
    }
}
