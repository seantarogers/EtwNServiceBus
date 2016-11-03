namespace Consumer.Extensions
{
    using System.Configuration;

    using Consumers;

    using Functions;

    using Producers;
    using Providers;

    using Easy.Logger;

    using SimpleInjector;
    using SimpleInjector.Extensions.LifetimeScoping;

    public static class ContainerExtensions
    {
        public static Container UseLifetimeScopeLifestyle(this Container container)
        {
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();
            return container;
        }

        public static Container RegisterComponents(this Container container)
        {
            RegisterFunctions(container);
            RegisterProducers(container);
            RegisterProviders(container);
            RegisterManagers(container);

            return container;
        }
        

        private static void RegisterManagers(Container container)
        {
            container.RegisterSingleton<ITraceSessionManager, TraceSessionManager>();
        }

        private static void RegisterProducers(Container container)
        {
            container.Register<IEventConsumer, EventConsumer>();
        }
        
        private static void RegisterFunctions(Container container)
        {
            container.RegisterSingleton<ILogService>(() => Log4NetService.Instance);
            container.RegisterSingleton<ILoggerBuilder, LoggerBuilder>();
            container.RegisterSingleton<IEventPayloadBuilder, EventPayloadBuilder>();
        }
        
        private static void RegisterProviders(Container container)
        {
            container.RegisterSingleton<ConfigurationProvider>();
            container.RegisterInitializer<ConfigurationProvider>(
                c =>
                    {
                        c.ConnectionString = ConfigurationManager.ConnectionStrings["TraceDatabase"].ConnectionString;
                        c.ScomEventSource = ConfigurationManager.AppSettings["ScomEventSource"];
                    });
        }
    }
}
