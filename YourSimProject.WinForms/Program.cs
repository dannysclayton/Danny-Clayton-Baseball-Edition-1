using System;
using System.Windows.Forms;

namespace YourSimProject.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            AppBootstrap.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMenuForm());
        }
    }
}
