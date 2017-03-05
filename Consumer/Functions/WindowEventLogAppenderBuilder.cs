using Consumer.Adapters;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace Consumer.Functions
{
    public class WindowEventLogAppenderBuilder : IWindowEventLogAppenderBuilder
    {
        private readonly IEventLogAdapter eventLogAdapter;

        public WindowEventLogAppenderBuilder(IEventLogAdapter eventLogAdapter)
        {
            this.eventLogAdapter = eventLogAdapter;
        }

        public IAppender Build(string applicationName)
        {
            const string logName = "EtwConsumerLog";
            if (!eventLogAdapter.SourceExists(applicationName))
            {
                eventLogAdapter.CreateEventSource(applicationName, logName);
            }

            const string appenderName = "WindowsEventLogAppender";
            var appender = new EventLogAppender
                               {
                                   Name = applicationName + appenderName,
                                   LogName = logName,
                                   ApplicationName = applicationName
                               };

            // only log errors to the windows event log, not debugs as these entries generate scom alerts
            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.Error };
            appender.AddFilter(levelMatchFilter);
            var denyAllFilter = new DenyAllFilter();
            appender.AddFilter(denyAllFilter);
            var layout = new PatternLayout { ConversionPattern = "%newline %level contents: %message" };

            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            return appender;
        }
    }
}