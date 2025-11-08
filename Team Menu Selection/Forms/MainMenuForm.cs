using System;
using System.Windows.Forms;

namespace TeamMenuSelection.Forms
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void btnExhibition_Click(object sender, EventArgs e)
        {
            var teamForm = new TeamSelectionForm();
            teamForm.ShowDialog();
        }

        private void btnSeason_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Season mode coming soon!");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Settings coming soon!");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
