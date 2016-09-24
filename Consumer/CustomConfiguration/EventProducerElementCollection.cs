namespace Consumer.CustomConfiguration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(EventProducerConfigurationElement))]
    public class EventProducerElementCollection : ConfigurationElementCollection
    {
        private const string PropertyName = "eventProducer";

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
            return new EventProducerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventProducerConfigurationElement)element).Name;
        }

        public EventProducerConfigurationElement this[int idx] => (EventProducerConfigurationElement)BaseGet(idx);
    }
}