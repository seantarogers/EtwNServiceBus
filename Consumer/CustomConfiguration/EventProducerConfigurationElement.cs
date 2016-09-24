namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventProducerConfigurationElement : ConfigurationElement
    {
        private const string NameProperty = "name";
        private const string EasyLogggerNameProperty = "easyLoggerName";
        private const string EventSourceProperty = "eventSource";
        private const string ApplicationNameProperty = "applicationName";

        [ConfigurationProperty(NameProperty, IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base[NameProperty]; }
            set { base[NameProperty] = value; }
        }

        [ConfigurationProperty(EasyLogggerNameProperty, IsRequired = true)]
        public string EasyLoggerName
        {
            get { return (string)base[EasyLogggerNameProperty]; }
            set { base[EasyLogggerNameProperty] = value; }
        }

        [ConfigurationProperty(EventSourceProperty, IsRequired = true)]
        public string EventSource
        {
            get { return (string)base[EventSourceProperty]; }
            set { base[EventSourceProperty] = value; }
        }

        [ConfigurationProperty(ApplicationNameProperty, IsRequired = true)]
        public string ApplicationName
        {
            get { return (string)base[ApplicationNameProperty]; }
            set { base[ApplicationNameProperty] = value; }
        }
    }
}