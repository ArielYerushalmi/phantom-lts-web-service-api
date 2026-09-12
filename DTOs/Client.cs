using Fleck;

namespace LtsWebServiceAPI.DTOs
{
    public class Client
    {
        //public Client AddClient()
        //{
        //    var client = new Client();
        //    return client;
        //}
        public IWebSocketConnection clientSocket { get; set; }

        public string[] parameterToSubscribe { get; set; }
    }
}
