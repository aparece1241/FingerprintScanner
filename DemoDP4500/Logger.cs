using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollment
{
    public enum LogType
    {
        INFO,
        ERROR,
        DEBUG
    }

    public class Logger
    {

        /*
         * Logger
         * 
         * param: LogType log
         * param: String message
         * param: classname
         * 
         * return void
         */
        public static void log(LogType log, string message, string classname)
        {
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string buildMessage = String.Format("[{0}]:{1}: {2}", date, classname, message);

            // limit string length
            if (buildMessage.Length > 1000)
            {
                buildMessage = buildMessage.Substring(0, 500) + "...]";
            }

            string filename = String.Format("{0}.log", date);
            string directory = @"\logs\";

            switch (log)
            {
                case LogType.DEBUG:
                    directory = directory + "debug";
                    break;
                case LogType.ERROR:
                    directory = directory + "error";
                    break;
                case LogType.INFO:
                    directory = directory + "info";
                    break;
            }

            FileHandler.FingerPrintLogger(buildMessage, filename, directory);
        }
    }
}
