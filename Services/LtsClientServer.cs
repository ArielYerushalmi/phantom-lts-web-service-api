using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhantomLibrary.Models;
using System;
using System.Collections.Generic;

namespace LtsWebServiceAPI.Services
{
    public class LtsClientServer : IDataReciever
    {
        private readonly string uri = "ws://127.0.0.1:5555";
        private string[] parametersToSub;
        private WebSocketServer clientWebSocketServer;
        private IWebSocketConnection clientSocket;
        private LtsTelemetryConsumer telemetryConsumer;

        public LtsClientServer()
        {
            clientWebSocketServer = new WebSocketServer(this.uri);
            telemetryConsumer = new LtsTelemetryConsumer(FilterData);
            clientSocket = null;
        }

        public void StartSocketServer()
        {
            try
            {
                clientWebSocketServer.Start(socket => // start client web socket
                {
                    socket.OnOpen = () =>
                    {
                        clientSocket = socket;
                        Console.WriteLine("client connected: ltsClient Websocket is open: port 5555");
                    };
                    socket.OnClose = () =>
                    {
                        socket.Close();
                        clientSocket = null;
                        Console.WriteLine("ltsClient Websocket is closed: port 5555");
                        telemetryConsumer.CloseConsumer();
                    };
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            ///
            /// After client is connected: connect the socket that milk pipeline outputs.
            /// it happened just in case a client is requesting for data.
            ///
        }

        public void Subscribe(string[] sub)
        {
            this.parametersToSub = sub;
            if (telemetryConsumer.IsConsumerActive())
            {
                telemetryConsumer.StartConsumer(); // init pipeline consumer
                Console.WriteLine($"Client subscribed: data recieved event is null ? {telemetryConsumer.DataRecievedFunc == null}");
            }
        }

        public void FilterData(JObject data)
        {
            JArray array = (JArray)data["Parameters"];
            IList<FrameParameter> parameter = array.ToObject<IList<FrameParameter>>();
            var parameterFromClient = this.parametersToSub;
            List<FrameParameter> listToSend = new List<FrameParameter>();

            foreach (FrameParameter p in parameter)
            {
                Console.WriteLine("all data is: " + p);
                foreach (string name in parameterFromClient)
                {
                    if (p.Name.Equals(name))
                    {
                        listToSend.Add(p);
                    }
                }
            }
            var jsonToSend = JsonConvert.SerializeObject(listToSend);
            SendData(jsonToSend);
        }

        public void SendData(string data)
        {
            if (clientSocket != null)
            {
                Console.WriteLine(data);
                this.clientSocket.Send(data);
                Console.WriteLine("ltsClient websocket send data ===>");
            }
            else
            {
                Console.WriteLine("ltsClient websocket working but is null |||");
            }
        }

        public void StopServer()
        {
            this.telemetryConsumer.CloseConsumer();
            this.clientWebSocketServer.Dispose();
        }
    }
}
