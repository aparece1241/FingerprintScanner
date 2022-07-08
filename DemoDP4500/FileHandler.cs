using System;
using System.IO;
using Enrollement;
using Newtonsoft.Json;

/// <summary>
/// Handle file creation and modifications(reading and writting)
/// </summary>
/// 
namespace Enrollment
{
    public class FileHandler
    {
        public static bool IsDataLoaded = false;
        protected static string currentUserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string path = currentUserPath + @"\AppData\Local\Tele-time\";

        public FileHandler() { }
        
        /*
         * Write or create the file(if doesnt exist)
         */
        public static void writeFile(string fileName, string data)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileStream fs = new FileStream(path + fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(data);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        /*
         * Read the file
         * 
         * return string
         */
        public static String readFile(string fileName, string directory = null)
        {
            string modifiedPath = path + directory + @"\" + fileName;
            if (directory != null)
            {
                if (!Directory.Exists(path + directory))
                {
                    Directory.CreateDirectory(path + directory);
                }
            } else
            {
                modifiedPath = path + fileName;
            }

            return File.Exists(modifiedPath) ? File.ReadAllText(modifiedPath) : "[]";
        }

        /*
         * Entry point of the file create and modification
         */
        public static void saveData(Model model)
        {
            if (RealTimeHandler.isInternetConnnected)
            {
                string serializedData = JsonConvert.SerializeObject(model.getData(), Formatting.Indented);
                string fileName = model.getFileName();
                FileHandler.writeFile(fileName, serializedData);
                Console.WriteLine("Creating or Modifying the file!");
            }
        }

        /*
         * Retrieve data from file using the Employee Model
         */
         public static EmployeeModel[] retrieveData(EmployeeModel model, string directory = null)
        {
            EmployeeModel[] data = JsonConvert.DeserializeObject<EmployeeModel[]>(readFile(model.getFileName(), directory));
            return data;
        }

        /*
         * Retrieve data from file using the Attendance Model
         */
        public static AttendanceModel[] retrieveData(AttendanceModel model, string directory = null)
        {
            AttendanceModel[] data = JsonConvert.DeserializeObject<AttendanceModel[]>(readFile(model.getFileName(), directory));
            return data;
        }

        /*
         *Retrieve date from file using the PendingAttendance model
         */
        public static PendingAttendance[] retrieveData(PendingAttendance model, string directory = null)
        {
            PendingAttendance[] data = JsonConvert.DeserializeObject<PendingAttendance[]>(readFile(model.getFileName(), directory));
            return data;
        }

        /*
         * Logs fingerprint activity
         * NOTE: Sole porpuse logging
         */
        public static void FingerPrintLogger(string data, string fileName, string directory, bool isAppend = true)
        {
            FileMode fileMode = isAppend ? FileMode.Append : FileMode.Create;

            if (!Directory.Exists(path+directory))
            {
                Directory.CreateDirectory(path + directory);
            }

            string logsPath = Path.Combine(path + directory + @"\", fileName);
            FileStream fs = new FileStream(logsPath, fileMode, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(data);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        /*
         * Validate file if exist or empty
         * @param string filename
         * @return Boolean
         */
        public static bool validateFile(string filename, string directory = null)
        {
            return readFile(filename, directory) != "[]";
        }
    }
}
