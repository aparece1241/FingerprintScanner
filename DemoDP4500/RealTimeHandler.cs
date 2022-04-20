
using System;
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
            Console.WriteLine(e.Topic);
            switch (e.Topic)
            {
                case "attendance":
                    Console.WriteLine("Please fire the attendance event!");
                    break;
                case "employee":
                    Console.WriteLine("Please fire the employee event!");
                    break;
                default:
                    break;
            }
        }
    }
}















