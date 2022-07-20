
using System;
using System.Net;
using Enrollment;
using System.Linq;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
//using Quobject.SocketIoClientDotNet.Client;

//using EngineIOSharp.Common.Enum;
//using SocketIOSharp.Common;
//using SocketIOSharp.Client;

using WebSocketSharp;

namespace Enrollment
{
    class RealTimeHandler
    {
        public static bool isInternetConnnected = false;
        protected byte[] QOSLevel = {
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
        };
        public static bool IsBrkoerConnected = true;

        protected Model model;
        public static MqttClient mqttClient;
        public static WebSocket ws;
        public Config config = new Config();

        #region altenative brokers
        //public RealTimeHandler(string host = "test.mosquitto.org")
        //public RealTimeHandler(string host = "mqtt.eclipseprojects.io")
        //public RealTimeHandler(string host = "mqtt://node02.myqtthub.com")
        #endregion alternative brokers

        public RealTimeHandler(string host = "broker.emqx.io")
        {
            try
            {

                // Websocketsharp
                ws = new WebSocket("ws://websocket-server-pro.herokuapp.com");
                
                ws.Connect();

                ws.OnMessage += Ws_OnMessage;
                ws.OnError += Ws_OnError;

                if (ws.IsAlive)
                {
                    Logger.log(LogType.DEBUG, "Successfully connected websocket!", this.GetType().Name);
                    isInternetConnnected = true;
                }

                #region Commented previous codes
                //SocketIOSharp
                //SocketIOClient client = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "localhost", 3000));


                //client.On(SocketIOEvent.CONNECTION, () => {
                //    Console.WriteLine("Connected!");
                //});

                //client.On(SocketIOEvent.ERROR, () => {
                //    Console.WriteLine("Connection error!");
                //});

                //client.On(SocketIOEvent.DISCONNECT, () => {
                //    Console.WriteLine("Connection Disconencted!");
                //});

                //client.On("message", () => Console.WriteLine("Message"));

                //client.Connect();

                // SocketIODotNet
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                //Console.WriteLine(config.SocketIoServer);
                //var socket = IO.Socket(config.SocketIoServer);

                //socket.On(Socket.EVENT_CONNECT, () =>
                //{
                //    Console.WriteLine("Connected!");
                //    Logger.log(LogType.DEBUG, "Socket server connected!", this.GetType().Name);
                //});

                //socket.On(Socket.EVENT_CONNECT_ERROR, (e) =>
                //{
                //    Console.WriteLine("Connection Error! " + e.ToString());
                //    Logger.log(LogType.ERROR, e.ToString(), this.GetType().Name);
                //});

                //socket.On(Socket.EVENT_CONNECT_TIMEOUT, (e) =>
                //{
                //    Console.WriteLine("Connection Timeout! " + e.ToString());
                //    Logger.log(LogType.ERROR, e.ToString(), this.GetType().Name);
                //});

                //mqttClient = new MqttClient(host);
                ////mqttClient.Connect("", "", "");
                //mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                //// Subscribe to topics
                //mqttClient.Subscribe(new string[] { "employee" }, this.QOSLevel);
                //mqttClient.Subscribe(new string[] { "attendance" }, this.QOSLevel);

                //// connect to mqtt serve
                //mqttClient.Connect("timetracker", "timetracker", "timetracker123");

                //// check if connected
                //if (!mqttClient.IsConnected)
                //{
                //    Console.WriteLine("Error in connecting to broker!");
                //    Logger.log(LogType.ERROR, "Failed to connect to broker", this.GetType().Name);
                //    IsBrkoerConnected = false;
                //    return;
                //}

                //isInternetConnnected = true;
                //Logger.log(LogType.DEBUG, "Successfully connected!", this.GetType().Name);
                #endregion
            }
            catch (Exception e)
            {
                Logger.log(LogType.ERROR, "Something wen't wrong!", this.GetType().Name);
                isInternetConnnected = false;
            }
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            Logger.log(LogType.ERROR, "Something wen't wrong in connecting websocket server!", this.GetType().Name);
            isInternetConnnected = false;
        }

        /*
         * Classify events recieve from websockets
         */
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            bool modelIssetFlg = false;
            switch (e.Data)
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

        #region mqtt call back method

        /*
         * Classify events recieve from mqtt
         */
        //private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        //{
        //    bool modelIssetFlg = false;
        //    switch (e.Topic)
        //    {
        //        case "attendance":
        //            Console.WriteLine("Please fire the attendance event!");
        //            this.model = new AttendanceModel();
        //            modelIssetFlg = true;
        //            break;
        //        case "employee":
        //            Console.WriteLine("Please fire the employee event!");
        //            this.model = new EmployeeModel();
        //            modelIssetFlg = true;
        //            break;
        //        default:
        //            break;
        //    }

        //    if (modelIssetFlg)
        //    {
        //        FileHandler.saveData(this.model);
        //    }
        //}
        #endregion

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
                    Logger.log(LogType.DEBUG, "Server is running!", "RealTimeHandler");
                } catch (Exception e)
                {
                    Logger.log(LogType.ERROR, "Server error!", "RealTimeHandler");
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
