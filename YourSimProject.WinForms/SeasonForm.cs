using System.Drawing;
using System.Windows.Forms;
using YourSimProject.Models;

namespace YourSimProject.WinForms
{
    public class SeasonForm : Form
    {
        public SeasonForm(GameEngine engine)
        {
            Text = "Season";
            ClientSize = new Size(600, 400);
            var lbl = new Label
            {
                Text = "Season management placeholder. Future: standings, schedule, simulate days.",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);
        }
    }
}
