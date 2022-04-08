using MassTransit;
using CommandService.Models;
using System.Threading.Tasks;
using CommandService;
using System.Collections.Generic;
using System;
using MediatR;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SimpleService.Handlers;
using CommandService.Models;
using System.Diagnostics;

namespace SimpleService.Consumers
{
    public class CommandManagerConsumer : IConsumer<SocketCommandMessage>
    {
        private readonly Dictionary<string, Tuple<Type, Type>> _methodContainer;
        private readonly IMediator _mediator;
        private readonly IBus _bus;
        public CommandManagerConsumer(IMediator mediator, IBus bus)
        {
            _methodContainer = CommonMethodsContainer.GetHandlerMethods();
            _mediator = mediator;
            _bus = bus;
        }
        public async Task Consume(ConsumeContext<SocketCommandMessage> context)
        {
            Trace.WriteLine("Heey");
            var _methodContainer = CommonMethodsContainer.GetHandlerMethods();
            var message = context.Message;
            if (_methodContainer.ContainsKey(message.MethodName))
            {
                var methodContainer = _methodContainer[message.MethodName];
                var requestType = methodContainer.Item1;
                var responseType = methodContainer.Item2;
                var desMethInfo = typeof(JsonConvert).GetMethods().Where(s => s.Name == "DeserializeObject" && s.IsGenericMethod).First();
                dynamic requestObject = desMethInfo.MakeGenericMethod(requestType).Invoke(null, new object[] { message.ParamsJson });
                dynamic response = await _mediator.Send(requestObject);
                var sendEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:socket-event"));
                await sendEndpoint.Send(new SocketMessage
                {
                    EventType = EventType.Reply,
                    CorrelationId = message.CorrelationId,
                    EventDataJson = JsonConvert.SerializeObject(response),
                    ConnectionId = message.ConnectionId,
                });
            }

        }
    }
}
