namespace Consumer.Consumers
{
    using System;

    public interface IEventConsumer
    {
        void Start();

        void OnError(Action<Exception> errorAction);
    }
}