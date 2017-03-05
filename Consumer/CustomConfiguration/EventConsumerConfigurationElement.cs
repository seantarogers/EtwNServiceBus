using System;
using System.Configuration;

namespace Consumer.CustomConfiguration
{
    public class EventConsumerConfigurationElement : ConfigurationElement, IEventConsumerConfigurationElement
    {
        private const string NameProperty = "name";
        private const string EventSourceProperty = "eventSource";
        private const string ApplicationNameProperty = "applicationName";
        private const string RollingLogPathProperty = "rollingLogPath";
        private const string EventTypeProperty = "eventType";

        [ConfigurationProperty(NameProperty, IsKey = true, IsRequired = true)]
        public string Name => (string)base[NameProperty];

        [ConfigurationProperty(EventSourceProperty, IsRequired = true)]
        public string EventSource => (string)base[EventSourceProperty];

        [ConfigurationProperty(ApplicationNameProperty, IsRequired = true)]
        public string ApplicationName => (string)base[ApplicationNameProperty];

        [ConfigurationProperty(RollingLogPathProperty, IsRequired = true)]
        public string RollingLogPath => (string)base[RollingLogPathProperty];

        [ConfigurationProperty(EventTypeProperty, IsRequired = true)]
        public EventType TraceEventType => (EventType)Enum.Parse(typeof(EventType), base[EventTypeProperty].ToString());
    }
}