namespace Consumer.Adapters
{
    using System;

    public class ContainerAdapter : IContainerAdapter
    {
        public IDisposable BeginLifetimeScope()
        {
            return ServiceHost.Container.BeginLifetimeScope();
        }

        public TComponent GetInstance<TComponent>() where TComponent : class
        {
            return ServiceHost.Container.GetInstance<TComponent>();
        }
    }
}