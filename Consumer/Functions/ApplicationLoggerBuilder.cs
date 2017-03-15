using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Consumer.CustomConfiguration;

namespace Consumer.Functions
{
    public class ApplicationLoggerBuilder : ILoggerBuilder
    {
        private readonly IAdoAppenderBuilder adoAppenderBuilder;
        private readonly IWindowEventLogAppenderBuilder windowEventLogAppenderBuilder;
        private readonly IRollingLogFileAppenderBuilder rollingLogFileAppenderBuilder;
        private readonly IBufferingForwardingAppenderBuilder bufferingForwardingAppenderBuilder;
        
        public ApplicationLoggerBuilder(
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

        public bool IsApplicable(EventType eventType) => eventType == EventType.Application;
        
        //All loggers used by consumers receive pre-formatted event payloads so we just need to map this into the logger
        //and do not need additional formatting of log message
        public ILog Build(string rollingLogFilePath, string consumerName, string applicationName)
        {
            LogLog.InternalDebugging = true;

            var log = LogManager.GetLogger(consumerName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            const string allLevelName = "ALL";
            logger.Level = logger.Hierarchy.LevelMap[allLevelName];

            const string rollingLogFileName = "application";
            var rollingFileAppender = rollingLogFileAppenderBuilder.Build(rollingLogFilePath, rollingLogFileName);

            var bufferingForwardingAppender = bufferingForwardingAppenderBuilder.Build();
            bufferingForwardingAppender.AddAppender(rollingFileAppender);
            logger.AddAppender(bufferingForwardingAppender);
            
            const string createInfoLogSprocName = "[usp_Create_InfoDebugLog]";
            var debugAdoAppender = adoAppenderBuilder.BuildForEventSource(Level.Debug, createInfoLogSprocName);
            logger.AddAppender(debugAdoAppender);

            const string createErrorSprocName = "[usp_Create_ErrorLog]";
            var errorAdoAppender = adoAppenderBuilder.BuildForEventSource(Level.Error, createErrorSprocName);
            logger.AddAppender(errorAdoAppender);

            var eventLogAppender = windowEventLogAppenderBuilder.Build(applicationName);
            logger.AddAppender(eventLogAppender);

            logger.Repository.Configured = true;

            return log;
        }
    }
}