using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace YourSimProject.WinForms
{
    public class ExhibitionForm : Form
    {
        public ExhibitionForm(GameEngine engine)
        {
            Text = "Exhibition Game";
            ClientSize = new Size(640, 400);
            var lbl = new Label
            {
                Text = $"Teams loaded: {engine.Teams?.Count ?? 0}. Future: select Home/Away and simulate.",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);
        }
    }
}
