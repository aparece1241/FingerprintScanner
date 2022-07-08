using System;
using Enrollement;
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

            //DataManager dt = new DataManager();
            //dt.getAttendancesWithinNDays();
            //Console.WriteLine(dt.FingerprintResponseMsg(257, new DateTime()));

            //EmployeeModel[] employees = dt.getEmployee(1);
            //string employeesJson = JsonConvert.SerializeObject(employees, Formatting.Indented);
            //FileHandler.writeFile(FileHandler.path + "test.json", employeesJson);
            //Console.WriteLine(FileHandler.readFile("test.json"));


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());

            //Application.Run(new frmRegistration());
            //Application.Run(new CaptureFormVerify());
        }
    }
}
