namespace Consumer.Subscribers
{
    using System;

    using Consumer.CustomConfiguration;

    public interface IEventSubscriber
    {
        void Start(EventSubscriberConfigurationElement eventSubscriberConfigurationElement);
        void Stop();
        void OnError(Action<Exception> errorAction);
    }
}