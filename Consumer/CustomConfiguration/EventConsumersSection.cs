using System.Configuration;

namespace Consumer.CustomConfiguration
{
    public class EventConsumersSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EventConsumersElementCollection EventConsumers => (EventConsumersElementCollection)base[""];

        public static EventConsumersSection Section => ConfigurationManager.GetSection("eventConsumersSection") as EventConsumersSection;
    }
}