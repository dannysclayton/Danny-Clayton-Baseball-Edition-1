using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YourSimProject.Services;
using YourSimProject.Models;

namespace TeamMenuSelection.Forms
{
    public partial class TeamSelectionForm : Form
    {
        private readonly TeamDatabase _teamDatabase = new();
        private Team? _teamA;
        private Team? _teamB;

        public TeamSelectionForm()
        {
            InitializeComponent();
            LoadTeams();
        }

        private void LoadTeams()
        {
            listBoxTeamA.Items.Clear();
            listBoxTeamB.Items.Clear();

            bool loaded = false;

            // 1. Try loading saved JSON database
            if (File.Exists("league_data.json"))
            {
                loaded = _teamDatabase.LoadDatabase();
            }

            // 2. Fallback: parse structure XLSX if present
            if (!loaded && File.Exists("Conference, Region, Team set up.xlsx"))
            {
                try
                {
                    _teamDatabase.LoadStructure("Conference, Region, Team set up.xlsx");
                    loaded = _teamDatabase.Conferences.Count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to parse structure file: {ex.Message}");
                }
            }

            // 3. Populate list boxes
            var allTeams = _teamDatabase.Conferences
                .SelectMany(c => c.Regions)
                .SelectMany(r => r.Teams)
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            if (allTeams.Count == 0)
            {
                listBoxTeamA.Items.AddRange(new[] { "No Teams Loaded" });
                listBoxTeamB.Items.AddRange(new[] { "No Teams Loaded" });
                MessageBox.Show("No team data found. Upload or generate league data first.");
                return;
            }

            listBoxTeamA.Items.AddRange(allTeams.ToArray());
            listBoxTeamB.Items.AddRange(allTeams.ToArray());
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            var teamAName = listBoxTeamA.SelectedItem?.ToString();
            var teamBName = listBoxTeamB.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(teamAName) || string.IsNullOrWhiteSpace(teamBName) || teamAName == teamBName)
            {
                MessageBox.Show("Please select two different teams.");
                return;
            }

            _teamA = _teamDatabase.GetTeam(teamAName);
            _teamB = _teamDatabase.GetTeam(teamBName);

            if (_teamA == null || _teamB == null)
            {
                MessageBox.Show("Selected team objects could not be resolved.");
                return;
            }

            // Launch gameplay placeholder
            using var playForm = new GamePlayForm(_teamA, _teamB);
            playForm.ShowDialog();
        }
    }
}
