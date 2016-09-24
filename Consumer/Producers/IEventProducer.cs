namespace Consumer.Producers
{
    using System;

    using CustomConfiguration;

    public interface IEventProducer
    {
        void Start(EventProducerConfigurationElement eventProducerConfigurationElement);
        void Stop();
        void OnError(Action<Exception> errorAction);
    }
}