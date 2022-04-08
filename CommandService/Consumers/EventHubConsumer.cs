using CommandService.Hubs;
using CommandService.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CommandService.Consumers
{
    public class EventHubConsumer : IConsumer<SocketMessage>
    {
        private readonly IHubContext<CommandHub> _hubContext;
        public EventHubConsumer(IHubContext<CommandHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task Consume(ConsumeContext<SocketMessage> context)
        {
            var message = context.Message;
            if (message.EventType == EventType.SSE)
            {
                var response = new SocketMessage
                {
                    EventDataJson = message.EventDataJson
                };
                await _hubContext.Clients.Groups(message.MatchGroups).SendAsync("onEvent",JsonConvert.SerializeObject(response));
            }
            else if(message.EventType == EventType.Reply)
            {
                var response = new SocketMessage
                {
                    EventDataJson = message.EventDataJson,
                    ConnectionId = message.ConnectionId,
                    CorrelationId = message.CorrelationId,
                };
                await _hubContext.Clients.Client(message.ConnectionId).SendAsync("onReply", JsonConvert.SerializeObject(response));
            }
        }
    }
}
