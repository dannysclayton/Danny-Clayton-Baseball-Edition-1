using System;
using System.Drawing;
using System.Windows.Forms;

namespace YourSimProject.WinForms
{
    public class MainMenuForm : Form
    {
        private Button btnSeason;
        private Button btnUpload;
        private Button btnExhibition;
        private Button btnSettings;
        private Button btnLoadSave;
        private Button btnExit;

        public MainMenuForm()
        {
            Text = "Baseball Sim - Main Menu";
            ClientSize = new Size(480, 360);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var title = new Label
            {
                Text = "BASEBALL SIMULATION",
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(110, 20)
            };
            Controls.Add(title);

            btnSeason = MakeButton("Season", 60, OnSeason);
            btnUpload = MakeButton("Upload", 100, OnUpload);
            btnExhibition = MakeButton("Exhibition Game", 140, OnExhibition);
            btnSettings = MakeButton("Settings", 180, OnSettings);
            btnLoadSave = MakeButton("Load Save", 220, OnLoadSave);
            btnExit = MakeButton("Exit", 260, OnExit);

            Controls.AddRange(new Control[] { btnSeason, btnUpload, btnExhibition, btnSettings, btnLoadSave, btnExit });
        }

        private Button MakeButton(string text, int top, EventHandler handler)
        {
            var b = new Button
            {
                Text = text,
                Width = 200,
                Height = 30,
                Location = new Point(140, top)
            };
            b.Click += handler;
            return b;
        }

    private void OnSeason(object? sender, EventArgs e) => new SeasonForm(AppBootstrap.Engine).ShowDialog(this);
    private void OnUpload(object? sender, EventArgs e) => new UploadForm().ShowDialog(this);
    private void OnExhibition(object? sender, EventArgs e) => new ExhibitionForm(AppBootstrap.Engine).ShowDialog(this);
    private void OnSettings(object? sender, EventArgs e) => new SettingsForm(AppBootstrap.Engine).ShowDialog(this);
    private void OnLoadSave(object? sender, EventArgs e) => new LoadSaveForm().ShowDialog(this);
        private void OnExit(object? sender, EventArgs e) => Close();

        private void Placeholder(string message)
        {
            MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
