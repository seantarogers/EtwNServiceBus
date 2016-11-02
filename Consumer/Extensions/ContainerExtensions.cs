namespace Consumer.Extensions
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;

    using Consumer.Adapters;
    using Consumer.Commands.Decorators;
    using Consumer.Commands.Handlers;
    using Consumer.Consumers;
    using Consumer.Dapper;
    using Consumer.Events;
    using Functions;

    using Producers;
    using Providers;
    using Queues;

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
            RegisterConsumers(container);
            RegisterProviders(container);
            RegisterCommands(container);
            RegisterFactories(container);
            RegisterAdapters(container);
            RegisterManagers(container);
            return container;
        }
        

        private static void RegisterManagers(Container container)
        {
            container.RegisterSingleton<ITraceSessionManager, TraceSessionManager>();
        }

        private static void RegisterProducers(Container container)
        {
            container.Register<IEventProducer, EventProducer>();
        }
        
        private static void RegisterConsumers(Container container)
        {
            RegisterCollectionOfSingletons<IEventConsumer>(container);
        }

        private static void RegisterAdapters(Container container)
        {
            container.RegisterSingleton<IContainerAdapter, ContainerAdapter>();
            container.RegisterSingleton<IEventQueue<TraceReceivedEvent>, EventQueue<TraceReceivedEvent>>();
        }

        private static void RegisterFactories(Container container)
        {
            container.RegisterSingleton<IDapperConnectionFactory, DapperConnectionFactory>();
        }

        private static void RegisterCommands(Container container)
        {
            container.Register(
                typeof (IOneWayCommandHandler<>),
                new List<Assembly> {typeof (CreateErrorLogCommandHandler).Assembly}, Lifestyle.Scoped);

            container.RegisterDecorator(
                typeof (IOneWayCommandHandler<>),
                typeof (OneWayAmbientUnitOfWorkDecorator<>), Lifestyle.Scoped);
        }

        private static void RegisterFunctions(Container container)
        {
            container.RegisterSingleton<ILogService>(() => Log4NetService.Instance);
            container.RegisterSingleton<ILogInitializer, LogInitializer>();

            container.Register<ICommandExecutor, CommandExecutor>(Lifestyle.Scoped);
            container.Register<IEventCommandExecutor, EventCommandExecutor>(Lifestyle.Scoped);
            container.RegisterSingleton<IEventPayloadBuilder, EventPayloadBuilder>();
        }

        private static void RegisterCollectionOfSingletons<TInterface>(Container container) where TInterface : class
        {
            // bit of extra code needed here to register a collection as singleton...
            var eventConsumerTypes = container.GetTypesToRegister(typeof(TInterface),
                new[] { typeof(TInterface).Assembly });

            var eventConsumerTypeRegistrations = (
                from eventConsumerType in eventConsumerTypes
                select Lifestyle.Singleton.CreateRegistration(eventConsumerType, container)
                ).ToArray();

            container.RegisterCollection<TInterface>(eventConsumerTypeRegistrations);
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
