using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleService.Handlers;
using MediatR;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SimpleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<SimpleResponse> Get(string methodName)
        {
            var _methodContainer = CommonMethodsContainer.GetHandlerMethods();
            if (_methodContainer.ContainsKey(methodName))
            {
                string paramsJson = "{Id:5}";
                var methodContainer = _methodContainer[methodName];
                var requestType = methodContainer.Item1;
                var responseType = methodContainer.Item2;
                var desMethInfo = typeof(JsonConvert).GetMethods().Where(s => s.Name == "DeserializeObject" && s.IsGenericMethod).First();
                dynamic requestObject = desMethInfo.MakeGenericMethod(requestType).Invoke(null, new object[] { paramsJson });
                var response = _mediator.Send(requestObject);

            }
            return new SimpleResponse { Data = "1" };

        }
    }
}
