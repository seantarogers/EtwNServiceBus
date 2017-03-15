using System;
using System.Configuration;
using Consumer.Adapters;
using Consumer.Consumers;
using Consumer.Functions;
using Consumer.Providers;
using log4net;
using SimpleInjector;
using Consumer.Controllers;
using Consumer.CustomConfiguration;

namespace Consumer.Extensions
{
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
            container.RegisterSingleton<ITraceEventSessionController, TraceEventSessionController>();
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
            container.Register<IBufferingForwardingAppenderBuilder, BufferingForwardingAppenderBuilder>();
            container.RegisterCollection<ILoggerBuilder>(new[] { typeof(ILoggerBuilder).Assembly });
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
                    c.FirstLevelBufferSizeInMb = int.Parse(ConfigurationManager.AppSettings["FirstLevelBufferSizeInMb"]);
                    c.SecondLevelBufferSizeInNumberOfEvents = int.Parse(ConfigurationManager.AppSettings["SecondLevelBufferSizeInNumberOfEvents"]);
                    c.RunLog4NetInDebugMode = bool.Parse(ConfigurationManager.AppSettings["RunLog4NetInDebugMode"]);
                    c.BufferFlushIntervalInSeconds = int.Parse(ConfigurationManager.AppSettings["BufferFlushIntervalInSeconds"]);
                });
        }
    }
}
