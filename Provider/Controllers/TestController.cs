using System;
using System.Threading.Tasks;
using System.Web.Http;
using Provider.EventSources;
using Infrastructure.Commands;
using NServiceBus;

namespace Provider.Controllers
{
    public class TestController : ApiController
    {
        private readonly IApplicationEventSource<TestController> applicationEventSource;
        private readonly IEndpointInstance endpointInstance;

        public TestController(IApplicationEventSource<TestController> applicationEventSource, IEndpointInstance endpointInstance)
        {
            this.applicationEventSource = applicationEventSource;
            this.endpointInstance = endpointInstance;
        }

        [Route("api/test")]
        public async Task<IHttpActionResult> Get()
        {
            applicationEventSource.DebugFormat("Application debug trace from the Test controller on {0}", DateTime.Now.ToShortDateString());
            applicationEventSource.ErrorFormat("Application error trace from the Test controller on {0}", DateTime.Now.ToShortDateString());

            await endpointInstance.Send(new DoSomethingCommand());
            
            return Ok();
        }
    }
}