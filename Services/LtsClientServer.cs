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

        // Both fields are written from the Fleck/Kestrel callback threads (OnOpen/OnClose,
        // HTTP request handling) and read from the pipeline consumer's receive-loop thread.
        // They're only ever swapped as a whole (never mutated in place), so `volatile`
        // reference assignment is enough to make writes visible across threads.
        private volatile IWebSocketConnection clientSocket;
        private volatile HashSet<string> subscribedParameters = new HashSet<string>();

        private WebSocketServer clientWebSocketServer;
        private readonly LtsTelemetryConsumer telemetryConsumer;

        public LtsClientServer()
        {
            telemetryConsumer = new LtsTelemetryConsumer(FilterData);
        }

        public void StartSocketServer()
        {
            try
            {
                // Fleck's WebSocketServer can't be reused once Dispose()'d (see StopServer),
                // so a fresh instance is required to support Stop -> Start cycles.
                clientWebSocketServer = new WebSocketServer(this.uri);
                clientWebSocketServer.Start(socket => // start client web socket
                {
                    socket.OnOpen = () =>
                    {
                        clientSocket = socket;
                        Console.WriteLine("client connected: ltsClient Websocket is open: port 5555");
                    };
                    socket.OnClose = () =>
                    {
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
            this.subscribedParameters = new HashSet<string>(sub ?? Array.Empty<string>());
            if (!telemetryConsumer.IsConsumerActive())
            {
                telemetryConsumer.StartConsumer(); // init pipeline consumer
                Console.WriteLine($"Client subscribed: data recieved event is null ? {telemetryConsumer.DataRecievedFunc == null}");
            }
        }

        public void FilterData(JObject data)
        {
            var array = data["Parameters"] as JArray;
            if (array == null)
            {
                return; // frame doesn't carry a "Parameters" array - nothing to filter/send
            }

            IList<FrameParameter> parameters = array.ToObject<IList<FrameParameter>>();
            var wanted = this.subscribedParameters;
            List<FrameParameter> listToSend = new List<FrameParameter>();

            foreach (FrameParameter p in parameters)
            {
                if (p?.Name != null && wanted.Contains(p.Name))
                {
                    listToSend.Add(p);
                }
            }

            var jsonToSend = JsonConvert.SerializeObject(listToSend);
            SendData(jsonToSend);
        }

        public void SendData(string data)
        {
            var socket = clientSocket;
            if (socket != null)
            {
                socket.Send(data);
            }
        }

        public void StopServer()
        {
            this.telemetryConsumer.CloseConsumer();
            this.clientWebSocketServer?.Dispose();
            this.clientSocket = null;
        }
    }
}
