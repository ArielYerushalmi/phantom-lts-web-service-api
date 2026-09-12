using LtsWebServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace LtsWebServiceAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public IDataReciever LtsClientService;

        public UserController(IDataReciever dataReciever)
        {
            LtsClientService ??= dataReciever;
        }

        [HttpPut("Start")]
        public HttpResponseMessage StartClientWebSocket()
        {
            Console.WriteLine("got start request");
            LtsClientService.StartSocketServer();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut("Stop")]
        public HttpResponseMessage StopClientWebSocket()
        {
            Console.WriteLine("stop client request");
            LtsClientService.StopServer();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut("Subscribe")]
        public HttpResponseMessage Subscribe([FromBody] string[] req)
        {
            LtsClientService.Subscribe(req);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
