using System;
using Autofac;
using NServiceBus;
using Topshelf;

namespace Endpoint
{
    public class ServiceHost : IServiceHost
    {
        private static IContainer Container { get; set; }
        private static IEndpointInstance EndpointInstance { get; set; }
        private static HostControl hostControl;

        public bool Start(HostControl topshelfHostControl)
        {
            hostControl = topshelfHostControl;
          
            try
            {
                CreateContainer();
                StartNServiceBusEndpoint();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception raised during start. Details: {exception}");
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            try
            {
                EndpointInstance.Stop()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception raised during stop. Details: {exception}");
            }

            return true;
        }

        private void StartNServiceBusEndpoint()
        {
            var endpointConfiguration = CreateEndpointConfiguration();

            EndpointInstance = NServiceBus.Endpoint.Start(endpointConfiguration).Result;

            var containerRegisterBusConfig = new ContainerBuilder();
            containerRegisterBusConfig.RegisterInstance(EndpointInstance);
            containerRegisterBusConfig.Update(Container);            
            
        }

        private static EndpointConfiguration CreateEndpointConfiguration()
        {
            var endpointConfiguration = new EndpointConfiguration("Endpoint");

            endpointConfiguration.SendFailedMessagesTo("Endpoint.Error");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(Container));
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            return endpointConfiguration;
        }

        private static void CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            Container = containerBuilder.Build();
        }        
    }
}