using System.Drawing;
using System.Windows.Forms;

namespace YourSimProject.WinForms
{
    public class SettingsForm : Form
    {
        public SettingsForm(GameEngine engine)
        {
            Text = "Settings";
            ClientSize = new Size(520, 320);
            var lbl = new Label
            {
                Text = "Settings placeholder. Future: toggle sound, export format, default save location.",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);
        }
    }
}
