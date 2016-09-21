namespace Consumer.CustomConfiguration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(EventSubscriberConfigurationElement))]
    public class EventSubscriberElementCollection : ConfigurationElementCollection
    {
        private const string PropertyName = "eventSubscriber";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => PropertyName;

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public override bool IsReadOnly()
        {
            return false;
        }
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new EventSubscriberConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventSubscriberConfigurationElement)element).Name;
        }

        public EventSubscriberConfigurationElement this[int idx] => (EventSubscriberConfigurationElement)BaseGet(idx);
    }
}