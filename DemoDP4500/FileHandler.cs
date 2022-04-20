using System;
using System.IO;

/// <summary>
/// Handle file creation and modifications(reading and writting)
/// </summary>
/// 
namespace Enrollment
{
    public class FileHandler
    {
        protected static string currentUserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string path = currentUserPath + @"\AppData\Local\Tele-time\";

        public FileHandler() { }

        // write or create file
        public static void writeFile(string fileName, string data)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileStream fs = new FileStream(path + fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(data);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        // read the file content
        public static String readFile(string fileName)
        {
            return File.ReadAllText(path + fileName);
        }
    }
}
