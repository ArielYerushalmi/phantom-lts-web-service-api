using Newtonsoft.Json.Linq;
using PhantomLibrary.Models;
using System;
using WebSocketSharp;

namespace LtsWebServiceAPI.Services
{
    public class LtsTelemetryConsumer
    {
        public LtsClientServer LtsClient;
        public WebSocket pipelineConsumer;
        private readonly string pipelineWebsocketUrl = "ws://127.0.0.1:6666";

        public delegate void DataRecieved(JObject data);
        public DataRecieved DataRecievedFunc;


        public LtsTelemetryConsumer(DataRecieved filterDataEvent)
        {
            DataRecievedFunc = filterDataEvent;
        }

        public void StartConsumer()
        {
            pipelineConsumer = new WebSocket(pipelineWebsocketUrl);
            try
            {
                pipelineConsumer.Connect();

                pipelineConsumer.OnOpen += (sender, e) =>
                {
                    Console.WriteLine("LTS Connected to Pipeline: port 6666");
                };

                pipelineConsumer.OnClose += (sender, e) =>
                {
                    Console.WriteLine("Pipeline connected is closed: port 6666");
                };

                pipelineConsumer.OnMessage += (sender, e) =>
                {
                    Console.WriteLine("LtsPipeLine Socket Got Data From PipeLine: port 6666\n");
                    DataRecievedFunc?.Invoke(JObject.Parse(e.Data));
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public bool IsConsumerActive()
        {
            if (pipelineConsumer == null || pipelineConsumer.IsAlive == false)
                return true;
            else
                return false;
        }

        public void CloseConsumer()
        {
            pipelineConsumer.Close();
        }
    }
}
