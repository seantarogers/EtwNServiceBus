namespace Provider.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    using EventSources;

    using Infrastructure.Commands;

    using NServiceBus;

    public class TestController : ApiController
    {
        private readonly IApplicationEventSource applicationEventSource;

        private readonly IEndpointInstance endpointInstance;

        public TestController(IApplicationEventSource applicationEventSource, IEndpointInstance endpointInstance)
        {
            this.applicationEventSource = applicationEventSource;
            this.endpointInstance = endpointInstance;
        }

        [Route("api/test")]
        public async Task<IHttpActionResult> Get()
        {
            applicationEventSource.DebugFormat(this, "Application debug trace from the Test controller on {0}", DateTime.Now.ToShortDateString());
            applicationEventSource.ErrorFormat(this, "Application debug from the Test controller on {0}", DateTime.Now.ToShortDateString());

            await endpointInstance.Send(new DoSomethingCommand());
            
            return Ok();
        }
    }
}