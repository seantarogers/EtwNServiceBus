namespace Consumer.Extensions
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;

    using Command.Decorators.Consumer.Commands.Decorators;
    using Command.Handlers;

    using Adapters;
    using Command.Dapper;

    using Consumers;

    using Functions;

    using Producers;
    using Providers;

    using Easy.Logger;

    using SimpleInjector;
    using SimpleInjector.Extensions.ExecutionContextScoping;

    public static class ContainerExtensions
    {
        public static Container UseExecutionContextLifestyle(this Container container)
        {
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
            return container;
        }

        public static Container RegisterComponents(this Container container)
        {
            RegisterAdapters(container);
            RegisterFunctions(container);
            RegisterProducers(container);
            RegisterProviders(container);
            RegisterManagers(container);
            RegisterCommands(container);
            RegisterFactories(container);

            return container;
        }

        private static void RegisterFactories(Container container)
        {
            container.Register<IDapperConnectionFactory, DapperConnectionFactory>(Lifestyle.Scoped);
        }

        private static void RegisterAdapters(Container container)
        {
            container.RegisterSingleton<IContainerAdapter, ContainerAdapter>();
        }

        private static void RegisterCommands(Container container)
        {
            container.Register(
                typeof(ITransactionalCommandHandler<>),
                new List<Assembly> { typeof(CreateErrorLogCommandHandler).Assembly }, Lifestyle.Scoped);

            container.Register(
                typeof(INonTransactionalCommandHandler<>),
                new List<Assembly> { typeof(CreateWindowsEventLogCommandHandler).Assembly }, Lifestyle.Scoped);

            container.RegisterDecorator(
                typeof(ITransactionalCommandHandler<>),
                typeof(UnitOfWorkDecorator<>), Lifestyle.Scoped);
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
