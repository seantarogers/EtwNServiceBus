using System.Configuration;

namespace Consumer.CustomConfiguration
{
    public class EventConsumerConfigurationElement : ConfigurationElement
    {
        private const string NameProperty = "name";
        private const string EventSourceProperty = "eventSource";
        private const string ApplicationNameProperty = "applicationName";

        [ConfigurationProperty(NameProperty, IsKey = true, IsRequired = true)]
        public string Name => (string)base[NameProperty];

        [ConfigurationProperty(EventSourceProperty, IsRequired = true)]
        public string EventSource => (string)base[EventSourceProperty];

        [ConfigurationProperty(ApplicationNameProperty, IsRequired = true)]
        public string ApplicationName => (string)base[ApplicationNameProperty];
    }
}