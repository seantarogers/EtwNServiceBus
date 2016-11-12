using System;
using System.Configuration;

namespace Consumer.CustomConfiguration
{
    [ConfigurationCollection(typeof(EventConsumerConfigurationElement))]
    public class EventConsumerElementCollection : ConfigurationElementCollection
    {
        private const string PropertyName = "eventConsumer";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => PropertyName;

        protected override bool IsElementName(string elementName) => elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);

        public override bool IsReadOnly() => false;

        protected override ConfigurationElement CreateNewElement() => new EventConsumerConfigurationElement();

        protected override object GetElementKey(ConfigurationElement element) => ((EventConsumerConfigurationElement)element).Name;

        public EventConsumerConfigurationElement this[int idx] => (EventConsumerConfigurationElement)BaseGet(idx);
    }
}