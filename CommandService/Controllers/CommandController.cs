using CommandService.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly IBus _bus;
        public CommandController(IBus bus)
        {
            _bus = bus;
        }
        [HttpGet("test")]
        public async Task<string> GetAsync()
        {
            var sendEndpoint = await _bus.GetSendEndpoint(new System.Uri($"exchange:command-exchange?type=topic"));
            await sendEndpoint.Send<SocketCommandMessage>(new SocketCommandMessage
            {
                ConnectionId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid(),
                MethodName = "GetData",
                ParamsJson = "{Id:1}",
                Topic = "Core.SimpleService"

            });
            return "Success";
        }
    }
}
