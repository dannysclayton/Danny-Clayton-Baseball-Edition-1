using System;
using System.Windows.Forms;
using YourSimProject.Models;

namespace TeamMenuSelection.Forms
{
    public partial class GamePlayForm : Form
    {
        private readonly Team _home;
        private readonly Team _away;

        public GamePlayForm(Team home, Team away)
        {
            _home = home;
            _away = away;
            InitializeComponent();
            lblMatchup.Text = $"{_away.Name} at {_home.Name}";
        }
    }
}
