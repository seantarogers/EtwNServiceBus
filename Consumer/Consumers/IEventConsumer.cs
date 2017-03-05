using System;

namespace Consumer.Consumers
{
    using Consumer.CustomConfiguration;

    public interface IEventConsumer
    {
        string Name { get; }
        void Start(IEventConsumerConfigurationElement eventConsumerConfigurationElement, Action<Exception> errorAction);
        void Stop();
    }
}