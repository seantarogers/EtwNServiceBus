namespace Provider
{
    using Autofac.Integration.WebApi;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new ApiHttpConfiguration();

            app.UseAutofacWebApi(httpConfiguration);
            app.UseWebApi(httpConfiguration);
            
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(ServiceHost.Container);
        }
    }
}