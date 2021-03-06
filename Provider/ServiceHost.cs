﻿using System;
using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using NServiceBus;
using NServiceBus.Features;
using Topshelf;
using NServiceBus.Logging;
using Provider.EtwLogger;
using Provider.EventSources;
using Owin;

namespace Provider
{
    public class ServiceHost : IServiceHost
    {
        private static IEndpointInstance EndpointInstance { get; set; }
        private static IContainer Container { get; set; }

        private static IDisposable owinHost;

        public bool Start(HostControl topshelfHostControl)
        {
            try
            {
                StartNServiceBusEndpoint();
                StartOwinWebHost();

                var applicationEventSource = new ApplicationEventSource<ServiceHost>();
                applicationEventSource.DebugFormat("successfully started event provider");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }

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
            owinHost = WebApp.Start(httpLocalhost,
                app =>
                    {

                        var httpConfiguration = new ApiHttpConfiguration();
                        app.UseAutofacWebApi(httpConfiguration);
                        app.UseWebApi(httpConfiguration);
                        httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
                    });

            Console.WriteLine("Successfully started the web host on: {0}", httpLocalhost);
        }

        private static void StartNServiceBusEndpoint()
        {
            SetUpNServiceBusEtwLogger();

            var endpointConfiguration = CreateSendOnlyEndpointConfiguration();
            EndpointInstance = Endpoint.Start(endpointConfiguration)
                .Result;

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            //generic singleton so each generic source can be quickly resolved after first use
            containerBuilder.RegisterGeneric(typeof(ApplicationEventSource<>))
                .As(typeof(IApplicationEventSource<>))
                .SingleInstance();

            containerBuilder.RegisterInstance(EndpointInstance);
            Container = containerBuilder.Build();
        }

        private static void SetUpNServiceBusEtwLogger()
        {
            var loggerDefinition = LogManager.Use<EtwLoggerDefinition>();
            loggerDefinition.Initialize(new BusEventSource(), LogLevel.Debug);
        }

        private static EndpointConfiguration CreateSendOnlyEndpointConfiguration()
        {
            var endpointConfiguration = new EndpointConfiguration("Provider");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            endpointConfiguration.SendOnly();
            return endpointConfiguration;
        }
    }
}