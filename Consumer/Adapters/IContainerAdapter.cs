namespace Consumer.Adapters
{
    using System;

    public interface IContainerAdapter
    {
        IDisposable BeginLifetimeScope();
        TComponent GetInstance<TComponent>() where TComponent : class;
    }
}