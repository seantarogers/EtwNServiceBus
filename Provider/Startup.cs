namespace Provider
{
    using Autofac;
    using Autofac.Core;
    using Autofac.Integration.WebApi;

    using Microsoft.Owin;

    using Owin;

    using Provider.EventSources;
    
    //[assembly: OwinStartup(typeof(Startup))]
    public class Startup
    {

        //public static IEndpointInstance EndpointInstance;

        public void Configuration(IAppBuilder app)
        {
            //var httpConfiguration = new ApiHttpConfiguration();
            //app.UseWebApi(httpConfiguration);

            //// set up container
            //var containerBuilder = new ContainerBuilder();
            //containerBuilder.RegisterComponents();
            //var container = containerBuilder.Build();
            //return container;

            //httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(EndpointConfig.Container);

            //var container = new Container();
            //container.RegisterSingleton<IApplicationEventSource, ApplicationEventSource>();
            //container.RegisterSingleton<IBusEventSource, BusEventSource>();
            //container.Verify();
        }
    }
}