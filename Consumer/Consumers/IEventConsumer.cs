namespace Consumer.Producers
{
    using System;

    using CustomConfiguration;

    public interface IEventConsumer
    {
        void Start(EventConsumerConfigurationElement eventConsumerConfigurationElement);
        void Stop();
        void OnError(Action<Exception> errorAction);
    }
}