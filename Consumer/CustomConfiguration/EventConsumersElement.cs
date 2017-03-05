using System;
    using System.Configuration;

namespace Consumer.CustomConfiguration
{
    [ConfigurationCollection(typeof(EventConsumerConfigurationElement))]
    public class EventConsumersElement : ConfigurationElementCollection
    {
        private const string PropertyName = "eventConsumer";
        private const string DeploymentLocationProperty = "deploymentLocation";

        [ConfigurationProperty(DeploymentLocationProperty, IsRequired = true, IsKey = true)]
        public DeploymentLocationType DeploymentLocation => (DeploymentLocationType)Enum.Parse(typeof(DeploymentLocationType), base[DeploymentLocationProperty].ToString());

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => PropertyName;

        protected override ConfigurationElement CreateNewElement() => new EventConsumerConfigurationElement();

        protected override object GetElementKey(ConfigurationElement element) => ((EventConsumerConfigurationElement)element).Name;

        public EventConsumerConfigurationElement this[int idx] => (EventConsumerConfigurationElement)BaseGet(idx);
    }
}