using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LtsWebServiceAPI.Services
{
    public class LtsTelemetryConsumer
    {
        public LtsClientServer LtsClient;
        public ClientWebSocket pipelineConsumer;
        private CancellationTokenSource receiveLoopCts;
        private readonly string pipelineWebsocketUrl = "ws://127.0.0.1:6666";

        public delegate void DataRecieved(JObject data);
        public DataRecieved DataRecievedFunc;


        public LtsTelemetryConsumer(DataRecieved filterDataEvent)
        {
            DataRecievedFunc = filterDataEvent;
        }

        public void StartConsumer()
        {
            pipelineConsumer = new ClientWebSocket();
            receiveLoopCts = new CancellationTokenSource();
            try
            {
                pipelineConsumer.ConnectAsync(new Uri(pipelineWebsocketUrl), CancellationToken.None).Wait();
                Console.WriteLine("LTS Connected to Pipeline: port 6666");
                _ = ReceiveLoop(receiveLoopCts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[8192];
            try
            {
                while (pipelineConsumer.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    using var ms = new System.IO.MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await pipelineConsumer.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Pipeline connected is closed: port 6666");
                            return;
                        }
                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    var data = Encoding.UTF8.GetString(ms.ToArray());
                    Console.WriteLine("LtsPipeLine Socket Got Data From PipeLine: port 6666\n");
                    DataRecievedFunc?.Invoke(JObject.Parse(data));
                }
            }
            catch (OperationCanceledException)
            {
                // expected on CloseConsumer()
            }
            catch (Exception ex)
            {
                Console.WriteLine("Pipeline receive loop error: " + ex.Message);
            }
        }

        public bool IsConsumerActive()
        {
            if (pipelineConsumer == null || pipelineConsumer.State != WebSocketState.Open)
                return true;
            else
                return false;
        }

        public void CloseConsumer()
        {
            receiveLoopCts?.Cancel();
            pipelineConsumer?.Abort();
        }
    }
}
