using Consumer.Consumers;
using Consumer.Functions;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace Consumer.Extensions
{
    public static class ContainerExtensions
    {
        public static void UseExecutionContextLifestyle(this Container container)
        {
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();            
        }

        public static void RegisterComponents(this Container container)
        {
            RegisterFunctions(container);
            RegisterProducers(container);
            RegisterManagers(container);            
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
            container.RegisterSingleton<IEventPayloadBuilder, EventPayloadBuilder>();
        }
    }
}
