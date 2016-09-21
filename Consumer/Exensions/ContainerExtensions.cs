using IEventConsumer = Consumer.Consumers.IEventConsumer;

namespace Consumer.Exensions
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;

    using Adapters;

    using Consumer.Queues;

    using Producers;
    using Subscribers;

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
            RegisterServices(container);
            RegisterSubscribers(container);
            RegisterProducers(container);
            RegisterConsumers(container);
            RegisterProviders(container);
            RegisterCommands(container);
            RegisterFactories(container);
            RegisterAdapters(container);

            return container;
        }

        private static void RegisterSubscribers(Container container)
        {
            container.Register<IEventSubscriber, EventSubscriber>();
        }

        private static void RegisterProducers(Container container)
        {
            container.Register<IErrorEventProducer, ErrorEventProducer>();
            container.RegisterCollection<IDebugEventProducer>(new[]
            {
                typeof(DebugEventProducer),
                typeof(DebugBusEventProducer)
            });
        }

        private static void RegisterConsumers(Container container)
        {
            RegisterCollectionOfSingletons<IEventConsumer>(container);
        }

        private static void RegisterAdapters(Container container)
        {
            container.RegisterSingleton<IContainerAdapter, ContainerAdapter>();
            container.RegisterSingleton<IErrorQueue, ErrorQueue>();
            container.RegisterSingleton<IDebugQueue, DebugQueue>();
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

        private static void RegisterServices(Container container)
        {
            container.RegisterSingleton<IAsiLogger, AsiLogger>();
            container.RegisterSingleton<ILogService>(() => Log4NetService.Instance);

            container.Register<ITraceEventCommandExecutor, TraceEventCommandExecutor>(Lifestyle.Scoped);
            container.Register<IEventPayloadBuilder, EventPayloadBuilder>(Lifestyle.Scoped);
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
            container.RegisterInitializer<ConfigurationProvider>(c =>
            {
                c.AuditConnectionString = ConfigurationManager.ConnectionStrings["PremiumFinanceAudit"].ConnectionString;
            });
        }
    }
}
