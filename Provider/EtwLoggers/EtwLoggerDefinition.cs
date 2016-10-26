namespace Provider.EtwLoggers
{
    using System;

    using NServiceBus.Logging;

    using Provider.EventSources;

    public class EtwLoggerDefinition : LoggingFactoryDefinition
    {
        private LogLevel thisLevel;
        private IBusEventSource thisBusEventSource;

        public void Initialize(IBusEventSource busEventSource, LogLevel level)
        {
            thisBusEventSource = busEventSource;
            thisLevel = level;
        }

        protected override ILoggerFactory GetLoggingFactory()
        {
            Validate();

            var etwLogFactory = new EtwLogFactory(thisLevel);
            etwLogFactory.Initialize(thisBusEventSource);
            return etwLogFactory;
        }

        private void Validate()
        {
            if (thisBusEventSource == null)
            {
                throw new ApplicationException("Bus EventSource Not Initialized");
            }
        }
    }
}