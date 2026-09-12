using Newtonsoft.Json.Linq;

namespace LtsWebServiceAPI.Services
{
    public interface IDataReciever
    {
        void StartSocketServer();
        void Subscribe(string[] sub);
        void FilterData(JObject data);
        void SendData(string data);
        void StopServer();
    }
}