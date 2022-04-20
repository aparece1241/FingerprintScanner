using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Enrollement
{
    class Config
    {
        public string company = ConfigurationSettings.AppSettings["Company"];
        public string ApiUrl = ConfigurationSettings.AppSettings["ApiUrl"];

        public string sshServer = ConfigurationSettings.AppSettings["SSHServer"];
        public string sshUserName = ConfigurationSettings.AppSettings["sshUserName"];
        public string sshPassword = ConfigurationSettings.AppSettings["sshPassword"];
        public int sshPort = Int32.Parse(ConfigurationSettings.AppSettings["sshPort"]);
        public string sshKey = ConfigurationSettings.AppSettings["sshKey"];

        public string dbServer = ConfigurationSettings.AppSettings["Server"];
        public string dbUserID = ConfigurationSettings.AppSettings["UserID"];
        public string dbPassword = ConfigurationSettings.AppSettings["Password"];
        public string dbDatabase = ConfigurationSettings.AppSettings["Database"];
        public int dbPort = Int32.Parse(ConfigurationSettings.AppSettings["Port"]);
    }
}
