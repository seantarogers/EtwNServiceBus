namespace Consumer.Adapters
{
    using System;

    public interface IContainerAdapter
    {
        IDisposable BeginExecutionContextScope();

        TComponent GetInstance<TComponent>() where TComponent : class;
    }
}