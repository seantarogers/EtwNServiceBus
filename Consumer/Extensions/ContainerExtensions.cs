using System;
using System.Configuration;
using Consumer.Adapters;
using Consumer.Consumers;
using Consumer.Functions;
using Consumer.Providers;
using log4net;
using SimpleInjector;

namespace Consumer.Extensions
{
    using Consumer.CustomConfiguration;

    public static class ContainerExtensions
    {
        public static Container RegisterComponents(this Container container)
        {
            RegisterAdapters(container);
            RegisterFunctions(container);
            RegisterConsumers(container);
            RegisterProviders(container);
            RegisterManagers(container);

            return container;
        }

        private static void RegisterAdapters(Container container)
        {
            container.Register<IEventLogAdapter, EventLogAdapter>();
        }

        private static void RegisterManagers(Container container)
        {
            container.RegisterSingleton<ITraceSessionManager, TraceSessionManager>();
        }

        private static void RegisterConsumers(Container container)
        {
            container.Register<IEventConsumer, EventConsumer>();
        }

        private static void RegisterFunctions(Container container)
        {
            container.Register<IAdoAppenderBuilder, AdoAppenderBuilder>();
            container.Register<IWindowEventLogAppenderBuilder, WindowEventLogAppenderBuilder>();
            container.Register<IRollingLogFileAppenderBuilder, RollingLogFileAppenderBuilder>();
            container.Register<IEventPayloadBuilder, EventPayloadBuilder>();
            container.Register<IConsumerLoggerBuilder, ConsumerLoggerBuilder>();
            container.Register<IBufferFlusher, BufferFlusher>();

            container.RegisterConditional(typeof(ILog),
                c => typeof(Log4NetAdapter<>).MakeGenericType(c.Consumer != null
                ? c.Consumer.ImplementationType : typeof(ServiceHost)),
                Lifestyle.Singleton,
                c => true);
        }

        private static void RegisterProviders(Container container)
        {
            container.RegisterSingleton<ConfigurationProvider>();
            container.RegisterInitializer<ConfigurationProvider>(
                c =>
                {
                    c.PremiumFinanceAuditConnectionString = ConfigurationManager.ConnectionStrings["Logging"].ConnectionString;
                    c.DeploymentLocation = (DeploymentLocationType)Enum.Parse(typeof(DeploymentLocationType), ConfigurationManager.AppSettings["DeploymentLocation"]);
                    c.LoggingBufferSize = int.Parse(ConfigurationManager.AppSettings["LoggingBufferSize"]);
                    c.RunLog4NetInDebugMode = bool.Parse(ConfigurationManager.AppSettings["RunLog4NetInDebugMode"]);
                    c.BufferFlushIntervalInSeconds = int.Parse(ConfigurationManager.AppSettings["BufferFlushIntervalInSeconds"]);
                });
        }
    }
}
