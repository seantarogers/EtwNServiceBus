using System;
using System.Reflection;

using Autofac;
using Autofac.Integration.WebApi;

using Microsoft.Owin.Hosting;

using NServiceBus;
using NServiceBus.Features;

using Topshelf;

namespace Provider
{
    using NServiceBus.Logging;

    using EtwLoggers;
    using EventSources;

    public class ServiceHost : IServiceHost
    {
        private static IEndpointInstance EndpointInstance { get; set; }
        public static IContainer Container { get; private set; }

        private static IDisposable owinHost;

        public bool Start(HostControl topshelfHostControl)
        {
            StartNServiceBusEndpoint();
            StartOwinWebHost();

            var applicationEventSource = new ApplicationEventSource();
            applicationEventSource.DebugFormat(this, "successfully started event provider");

            return true;
        }

        public bool Stop()
        {
            owinHost.Dispose();
            EndpointInstance?.Stop().GetAwaiter().GetResult();
            return true;
        }

        private static void StartOwinWebHost()
        {
            const string httpLocalhost = "http://localhost:8089";
            owinHost = WebApp.Start(httpLocalhost);
            Console.WriteLine("Successfully started the SignalR publisher on: {0}", httpLocalhost);
        }

        private static void StartNServiceBusEndpoint()
        {
            Container = CreateContainer();

            SetUpNServiceBusEtwLogger();
            var endpointConfiguration = CreateEndpointConfiguration(Container);
            EndpointInstance = Endpoint.Start(endpointConfiguration).Result;

            var containerRegisterBusConfig = new ContainerBuilder();
            containerRegisterBusConfig.RegisterInstance(EndpointInstance);
            containerRegisterBusConfig.Update(Container);
        }

        private static void SetUpNServiceBusEtwLogger()
        {
            var loggerDefinition = LogManager.Use<EtwLoggerDefinition>();
            loggerDefinition.Initialize(new BusEventSource(), LogLevel.Debug);
        }

        private static EndpointConfiguration CreateEndpointConfiguration(IContainer container)
        {
            var endpointConfiguration = new EndpointConfiguration("Saga");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));
            endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Events"));
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            endpointConfiguration.SendOnly();
            return endpointConfiguration;
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            return containerBuilder.Build();
        }
    }
}