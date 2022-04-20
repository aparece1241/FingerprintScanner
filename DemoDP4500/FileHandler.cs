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

            FileStream fs = new FileStream(path + fileName, FileMode.OpenOrCreate, FileAccess.Write);
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
        public static String readFile(string fileName)
        {
            return File.ReadAllText(path + fileName);
        }

        /*
         * Entry point of the file create and modification
         */
        public static void saveData(Model model)
        {
            Console.WriteLine("Creating or Modifying the file!");
            string serializedData = JsonConvert.SerializeObject(model.getData(), Formatting.Indented);
            string fileName = model.getFileName();
            FileHandler.writeFile(fileName, serializedData);   
        }
    }
}
