namespace Provider.Controllers
{
    using System;
    using System.Web.Http;

    using EventSources;

    public class TestController : ApiController
    {
        private readonly IApplicationEventSource applicationEventSource;

        public TestController(IApplicationEventSource applicationEventSource)
        {
            this.applicationEventSource = applicationEventSource;
        }

        [Route("api/test")]
        public IHttpActionResult Get()
        {
            applicationEventSource.DebugFormat(this, "Debug statement from the Test controller on {0}", DateTime.Now.ToShortDateString());
            applicationEventSource.ErrorFormat(this, "Error statement from the Test controller on {0}", DateTime.Now.ToShortDateString());
            
            return Ok();
        }
    }
}