using System;
using System.Collections.Generic;

namespace CommandService.Models
{
    public enum EventType
    { 
        SSE = 0,
        Reply = 1,
    }

    public class SocketMessage
    {
        public EventType EventType { get; set; }
        public List<string> MatchGroups { get; set; }
        public string EventDataJson { get; set; }
        public Guid CorrelationId { get; set; }
        public string ConnectionId { get; set; }

    }

    public class SocketCommandMessage
    {
        public string Type { get; set; }
        public string Stream { get; set; }
        public string Service { get; set; }
        public string MethodName { get; set; }
        public string ParamsJson { get; set; }
        public Guid CorrelationId { get; set; }
        public string ConnectionId { get; set; }
        public string Topic { get; set; }
    }
}
