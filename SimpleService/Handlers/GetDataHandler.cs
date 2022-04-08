using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleService.Handlers
{
    public class SimpleRequest : IRequest<SimpleResponse>
    {
        public int Id { get; set; }
    }
    public class SimpleResponse
    {
        public string Data { get; set; }
    }

    public class GetDataHandler : IRequestHandler<SimpleRequest, SimpleResponse>
    {
        private readonly Dictionary<int, string> data = new Dictionary<int, string>()
        {
            {0,"Some Data from 0 index"},
            {1,"Some Data from 1 index"},
            {2,"Some Data from 2 index"},
            {3,"Some Data from 3 index"},
            {4,"Some Data from 4 index"},
        };
        public Task<SimpleResponse> Handle(SimpleRequest request, CancellationToken cancellationToken)
        {
            data.TryGetValue(request.Id, out var response);
            response = response ?? "Not found";
            return Task.FromResult(new SimpleResponse { Data = response });
        }
    }
}
