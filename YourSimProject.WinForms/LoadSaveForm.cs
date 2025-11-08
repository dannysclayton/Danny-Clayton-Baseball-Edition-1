using System.Drawing;
using System.Windows.Forms;

namespace YourSimProject.WinForms
{
    public class LoadSaveForm : Form
    {
        public LoadSaveForm()
        {
            Text = "Load / Save";
            ClientSize = new Size(500, 300);
            var lbl = new Label
            {
                Text = "Load/Save placeholder. Future: list saves, load selected, create new save.",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);
        }
    }
}
