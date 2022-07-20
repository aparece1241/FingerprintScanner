using System;
using Enrollment;
using Newtonsoft.Json;
using System.Windows.Forms;



namespace Enrollment
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // initialized socket events
            new RealTimeHandler();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());

            //Application.Run(new frmRegistration());
            //Application.Run(new CaptureFormVerify());
        }
    }
}
