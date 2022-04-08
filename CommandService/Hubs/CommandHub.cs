using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CommandService.Models;
using MassTransit;
using Newtonsoft.Json;

namespace CommandService.Hubs
{
    public class CommandHub : Hub
    {
        private IBus _bus;
        public CommandHub(IBus bus)
        {
            _bus = bus;
        }
        public async Task Subscribe(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task UnSubscribe(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task Exec(string message)
        {
            var request = JsonConvert.DeserializeObject<SocketCommandMessage>(message);
            var sendEndpoint = await _bus.GetSendEndpoint(new System.Uri($"exchange:command-exchange?type=topic"));
            await sendEndpoint.Send<SocketCommandMessage>(new SocketCommandMessage
            {
                ConnectionId = Context.ConnectionId,
                CorrelationId = request.CorrelationId,
                MethodName = request.MethodName,
                ParamsJson = request.ParamsJson,
                Service = request.Service,
                Stream = request.Stream,
                Type = request.Type,
                Topic = $"{request.Type}.{request.Stream}.{request.Service ?? "#"}"
            });
        }

        public async  Task Test()
        {
            await Clients.Caller.SendAsync("Test", "Hello");
        }
    }
}
