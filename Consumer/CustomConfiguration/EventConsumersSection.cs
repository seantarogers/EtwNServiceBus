namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventConsumersSection : ConfigurationSection
    {
        private const string EventConsumersProperty = "eventConsumers";
        private const string EventConsumersSectionName = "eventConsumersSection";

        public static EventConsumersSection Section { get; } = ConfigurationManager.GetSection(EventConsumersSectionName) as EventConsumersSection;

        [ConfigurationProperty(EventConsumersProperty)]
        public EventConsumerElementCollection EventConsumerElements => (EventConsumerElementCollection) base[EventConsumersProperty];
    }
}   