
using System;
using Enrollement;
using System.Linq;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Enrollment
{
    class RealTimeHandler
    {
        public static bool isInternetConnnected = false;
        public static bool isMqttConnected = false;
        protected byte[] QOSLevel = {
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
        };
        public static bool IsBrkoerConnected = true;

        public static MqttClient mqttClient;
        protected Model model;

        #region altenative brokers
        //public RealTimeHandler(string host = "test.mosquitto.org")
        //public RealTimeHandler(string host = "mqtt.eclipseprojects.io")
        //public RealTimeHandler(string host = "mqtt://node02.myqtthub.com")
        #endregion alternative brokers

        public RealTimeHandler(string host = "broker.emqx.io")
        {
            try
            {
                mqttClient = new MqttClient(host);
                mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                // Subscribe to topics
                mqttClient.Subscribe(new string[] { "employee" }, this.QOSLevel);
                mqttClient.Subscribe(new string[] { "attendance" }, this.QOSLevel);

                // connect to mqtt serve
                mqttClient.Connect("timetracker", "timetracker", "timetracker123");

                // check if connected
                if (!mqttClient.IsConnected)
                {
                    Console.WriteLine("Error in connecting to broker!");
                    IsBrkoerConnected = false;
                    return;
                }

                isMqttConnected = true;
                isInternetConnnected = true;
            }
            catch (Exception e)
            {
                isInternetConnnected = false;
                isMqttConnected = false;
            }
        }

        /*
         * Classify events recieve
         */
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            bool modelIssetFlg = false;
            switch (e.Topic)
            {
                case "attendance":
                    Console.WriteLine("Please fire the attendance event!");
                    this.model = new AttendanceModel();
                    modelIssetFlg = true;
                    break;
                case "employee":
                    Console.WriteLine("Please fire the employee event!");
                    this.model = new EmployeeModel();
                    modelIssetFlg = true;
                    break;
                default:
                    break;
            }

            if (modelIssetFlg)
            {
                FileHandler.saveData(this.model);
            }
        }

        /*
         * Check's the internet connection by pinging google
         */
        public static void InternetChecker()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() => {
                try
                {
                    DataManager dal = new DataManager();
                    dal.getConnection();
                    isInternetConnnected = true;
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    isInternetConnnected = false;
                }
            });
        }

        public static void SavingPendingAttendance()
        {
            bool response = false;
            PendingAttendance [] pendingAttendances = FileHandler.retrieveData(new PendingAttendance(), "pendings");
            PendingAttendance[] pendingAttendancesCopy = pendingAttendances;
            if (pendingAttendances.Length > 0)
            {
                for(int ndx = 0;ndx < pendingAttendances.Length;ndx++)
                {
                    response = (new DataManager()).SaveBatchAttendance(pendingAttendances[ndx].emp_id, pendingAttendances[ndx].date);
                    if (response)
                    {
                        pendingAttendancesCopy = pendingAttendancesCopy.Where(att => att.emp_id != pendingAttendances[ndx].emp_id).ToArray();
                    }
                }

                string serializedData = JsonConvert.SerializeObject(pendingAttendancesCopy, Formatting.Indented);
                FileHandler.FingerPrintLogger(serializedData, pendingAttendances[0].getFileName(), "pendings", false);
            }
        }
    }
}
