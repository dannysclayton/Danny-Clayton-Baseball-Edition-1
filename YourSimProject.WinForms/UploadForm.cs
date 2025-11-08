using System.Drawing;
using System.Windows.Forms;

namespace YourSimProject.WinForms
{
    public class UploadForm : Form
    {
        public UploadForm()
        {
            Text = "Upload";
            ClientSize = new Size(500, 300);
            var lbl = new Label
            {
                Text = "Upload placeholder. Future: select CSV/XLSX to import teams/players.",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);
        }
    }
}
