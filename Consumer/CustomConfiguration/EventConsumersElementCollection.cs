using System.Configuration;

namespace Consumer.CustomConfiguration
{
    public class EventConsumersElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement() => new EventConsumersElement();

        protected override object GetElementKey(ConfigurationElement element) => ((EventConsumersElement)element).DeploymentLocation;

        protected override string ElementName => "eventConsumers";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
    }
}
