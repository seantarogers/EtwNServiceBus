using Consumer.Providers;
using log4net.Appender;
using log4net.Core;

namespace Consumer.Functions
{
    public class BufferingForwardingAppenderBuilder : IBufferingForwardingAppenderBuilder
    {
        private readonly ConfigurationProvider configurationProvider;

        public BufferingForwardingAppenderBuilder(ConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        public BufferingForwardingAppender Build()
        {
            var bufferingForwardingAppender = new BufferingForwardingAppender
            {
                BufferSize = configurationProvider.SecondLevelBufferSizeInNumberOfEvents,
                Fix = FixFlags.None, // all logs are created in the provider so no volitile fields
                Lossy = false
            };
            bufferingForwardingAppender.ActivateOptions();
            return bufferingForwardingAppender;
        }
    }
}