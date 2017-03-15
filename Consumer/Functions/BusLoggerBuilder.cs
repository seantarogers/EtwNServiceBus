using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Consumer.CustomConfiguration;

namespace Consumer.Functions
{
    public class BusLoggerBuilder : ILoggerBuilder
    {
        private readonly IAdoAppenderBuilder adoAppenderBuilder;
        private readonly IWindowEventLogAppenderBuilder windowEventLogAppenderBuilder;
        private readonly IRollingLogFileAppenderBuilder rollingLogFileAppenderBuilder;
        private readonly IBufferingForwardingAppenderBuilder bufferingForwardingAppenderBuilder;
        
        public BusLoggerBuilder(
            IAdoAppenderBuilder adoAppenderBuilder,
            IWindowEventLogAppenderBuilder windowEventLogAppenderBuilder,
            IRollingLogFileAppenderBuilder rollingLogFileAppenderBuilder,
            IBufferingForwardingAppenderBuilder bufferingForwardingAppenderBuilder)
        {
            this.adoAppenderBuilder = adoAppenderBuilder;
            this.windowEventLogAppenderBuilder = windowEventLogAppenderBuilder;
            this.rollingLogFileAppenderBuilder = rollingLogFileAppenderBuilder;
            this.bufferingForwardingAppenderBuilder = bufferingForwardingAppenderBuilder;
        }

        public bool IsApplicable(EventType eventType) => eventType == EventType.Bus;

        public ILog Build(string rollingLogFilePath, string consumerName, string applicationName)
        {
            LogLog.InternalDebugging = true;

            var log = LogManager.GetLogger(consumerName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            const string allLevelName = "ALL";
            logger.Level = logger.Hierarchy.LevelMap[allLevelName];

            const string rollingLogFileName = "bus";
            var rollingFileAppender = rollingLogFileAppenderBuilder.Build(rollingLogFilePath, rollingLogFileName);

            var bufferingForwardingAppender = bufferingForwardingAppenderBuilder.Build();
            bufferingForwardingAppender.AddAppender(rollingFileAppender);
            logger.AddAppender(bufferingForwardingAppender);

            // only log errors to sql for the high traffic event sources (bus debug etc) 
            // so do not log debug statements to sql as it will be too noisy
            const string createErrorSprocName = "[usp_Create_ErrorLog]";
            var adoNetAppender = adoAppenderBuilder.BuildForEventSource(Level.Error, createErrorSprocName);
            logger.AddAppender(adoNetAppender);

            var eventLogAppender = windowEventLogAppenderBuilder.Build(applicationName);
            logger.AddAppender(eventLogAppender);

            logger.Repository.Configured = true;

            return log;
        }
    }
}