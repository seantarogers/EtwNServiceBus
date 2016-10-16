namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventProducersSection : ConfigurationSection
    {
        private const string EventProducersProperty = "eventProducers";
        private const string EventProducersSectionName = "eventProducersSection";

        public static EventProducersSection Section { get; } = ConfigurationManager.GetSection(EventProducersSectionName) as EventProducersSection;

        [ConfigurationProperty(EventProducersProperty)]
        public EventProducerElementCollection EventProducerElements => (EventProducerElementCollection) base[EventProducersProperty];
    }
}   