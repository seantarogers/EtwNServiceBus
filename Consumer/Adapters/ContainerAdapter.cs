namespace Consumer.Adapters
{
    using System;

    using SimpleInjector.Extensions.ExecutionContextScoping;

    public class ContainerAdapter : IContainerAdapter
    {
        public IDisposable BeginExecutionContextScope() => ServiceHost.Container.BeginExecutionContextScope();

        public TComponent GetInstance<TComponent>() where TComponent : class => ServiceHost.Container.GetInstance<TComponent>();
    }
}