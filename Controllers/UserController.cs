using LtsWebServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LtsWebServiceAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDataReciever LtsClientService;

        public UserController(IDataReciever dataReciever)
        {
            LtsClientService = dataReciever;
        }

        [HttpPut("Start")]
        public IActionResult StartClientWebSocket()
        {
            Console.WriteLine("got start request");
            LtsClientService.StartSocketServer();
            return Ok();
        }

        [HttpPut("Stop")]
        public IActionResult StopClientWebSocket()
        {
            Console.WriteLine("stop client request");
            LtsClientService.StopServer();
            return Ok();
        }

        [HttpPut("Subscribe")]
        public IActionResult Subscribe([FromBody] string[] req)
        {
            if (req == null)
            {
                return BadRequest("Expected a JSON array of parameter names to subscribe to.");
            }

            LtsClientService.Subscribe(req);
            return Ok();
        }
    }
}
