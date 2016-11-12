namespace Consumer.Consumers
{
    using System;

    using CustomConfiguration;

    public interface IEventConsumer
    {
        void Start(EventConsumerConfigurationElement eventConsumerConfigurationElement, Action<Exception> raiseException);
        void Stop();
    }
}