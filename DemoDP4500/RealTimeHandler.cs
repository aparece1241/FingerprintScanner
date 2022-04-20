
using System;
using Enrollement;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Enrollment
{
    class RealTimeHandler
    {
        protected byte[] QOSLevel = {
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
        };

        protected MqttClient mqttClient;
        protected Model model;

        public RealTimeHandler(string host)
        {
            mqttClient = new MqttClient(host);
            mqttClient.MqttMsgPublishReceived  += MqttClient_MqttMsgPublishReceived;

            // Subscribe to topics
            mqttClient.Subscribe(new string[] { "employee" }, this.QOSLevel);
            mqttClient.Subscribe(new string[] { "attendance" }, this.QOSLevel);

            // connect to mqtt serve
            mqttClient.Connect("timetracker");

            // check if connected
            if (!mqttClient.IsConnected)
            {
                Console.WriteLine("Error in connecting to broker!");
                return;
            }
        }

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
    }
}















