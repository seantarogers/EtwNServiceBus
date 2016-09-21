namespace Consumer.CustomConfiguration
{
    using System.Configuration;

    public class EventSubscribersSection : ConfigurationSection
    {
        private const string EventSubscribersProperty = "eventSubscribers";
        private const string EventSubscribersSectionName = "eventSubscribersSection";

        public static EventSubscribersSection Section { get; } = ConfigurationManager.GetSection(EventSubscribersSectionName) as EventSubscribersSection;

        [ConfigurationProperty(EventSubscribersProperty)]
        public EventSubscriberElementCollection EventSubscriberElements
        {
            get { return (EventSubscriberElementCollection) base[EventSubscribersProperty]; }
            set { base[EventSubscribersProperty] = value; }
        }
    }
}   