namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventConsumerConfigurationElement : ConfigurationElement
    {
        private const string NameProperty = "name";
        private const string EventSourceProperty = "eventSource";
        private const string ApplicationNameProperty = "applicationName";
        private const string RollingLogPathProperty = "rollingLogPath";
        private const string RollingLogFileNameProperty = "rollingLogFileName";
        private const string LogDebugTracesToDatabaseProperty = "logDebugTracesToDatabase";

        [ConfigurationProperty(NameProperty, IsKey = true, IsRequired = true)]
        public string Name => (string)base[NameProperty];

        [ConfigurationProperty(EventSourceProperty, IsRequired = true)]
        public string EventSource => (string)base[EventSourceProperty];

        [ConfigurationProperty(ApplicationNameProperty, IsRequired = true)]
        public string ApplicationName => (string)base[ApplicationNameProperty];

        [ConfigurationProperty(RollingLogPathProperty, IsRequired = true)]
        public string RollingLogPath => (string)base[RollingLogPathProperty];

        [ConfigurationProperty(RollingLogFileNameProperty, IsRequired = true)]
        public string RollingLogFileName => (string)base[RollingLogFileNameProperty];

        [ConfigurationProperty(LogDebugTracesToDatabaseProperty, IsRequired = true)]
        public bool LogDebugTracesToDatabase => (bool)base[LogDebugTracesToDatabaseProperty];
    }
}