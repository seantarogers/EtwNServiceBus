namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventProducerConfigurationElement : ConfigurationElement
    {
        private const string NameProperty = "name";
        private const string EventSourceProperty = "eventSource";
        private const string ApplicationNameProperty = "applicationName";
        private const string LogPathProperty = "logPath";
        private const string LogDebugTracesToDatabaseProperty = "logDebugTracesToDatabase";

        [ConfigurationProperty(NameProperty, IsKey = true, IsRequired = true)]
        public string Name => (string)base[NameProperty];

        [ConfigurationProperty(EventSourceProperty, IsRequired = true)]
        public string EventSource => (string)base[EventSourceProperty];

        [ConfigurationProperty(ApplicationNameProperty, IsRequired = true)]
        public string ApplicationName => (string)base[ApplicationNameProperty];

        [ConfigurationProperty(LogPathProperty, IsRequired = true)]
        public string LogPath => (string)base[LogPathProperty];

        [ConfigurationProperty(LogDebugTracesToDatabaseProperty, IsRequired = true)]
        public bool LogDebugTracesToDatabase => (bool)base[LogDebugTracesToDatabaseProperty];
    }
}