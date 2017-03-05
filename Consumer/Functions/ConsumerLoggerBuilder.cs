using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace Consumer.Functions
{
    public class ConsumerLoggerBuilder : IConsumerLoggerBuilder
    {
        private readonly IAdoAppenderBuilder adoAppenderBuilder;
        private readonly IWindowEventLogAppenderBuilder windowEventLogAppenderBuilder;
        private readonly IRollingLogFileAppenderBuilder rollingLogFileAppenderBuilder;

        private const string CreateErrorSprocName = "[usp_Create_ErrorLog]";
        private const string CreateInfoLogSprocName = "[usp_Create_InfoDebugLog]";
        private const string AllLevelName = "ALL";

        public ConsumerLoggerBuilder(
            IAdoAppenderBuilder adoAppenderBuilder, 
            IWindowEventLogAppenderBuilder windowEventLogAppenderBuilder, 
            IRollingLogFileAppenderBuilder rollingLogFileAppenderBuilder)
        {
            this.adoAppenderBuilder = adoAppenderBuilder;
            this.windowEventLogAppenderBuilder = windowEventLogAppenderBuilder;
            this.rollingLogFileAppenderBuilder = rollingLogFileAppenderBuilder;
        }
        
        public ILog BuildForBusTracing(
            string rollingLogFilePath,
            string consumerName,
            string applicationName)
        {
            LogLog.InternalDebugging = true;

            var log = LogManager.GetLogger(consumerName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            logger.Level = logger.Hierarchy.LevelMap[AllLevelName];
            
            const string rollingLogFileName = "bus";
            var rollingFileAppender = rollingLogFileAppenderBuilder.Build(rollingLogFilePath, rollingLogFileName);
            logger.AddAppender(rollingFileAppender);

            // only log errors to sql for the high traffic event sources (signalr, bus debug etc) 
            // so do not log debug statements to sql as it will be too noisy
            var adoNetAppender = adoAppenderBuilder.BuildForEventSource(Level.Error, CreateErrorSprocName);
            logger.AddAppender(adoNetAppender);
            
            var eventLogAppender = windowEventLogAppenderBuilder.Build(applicationName);
            logger.AddAppender(eventLogAppender); 
            
            logger.Repository.Configured = true;

            return log;
        }

        public ILog BuildForSignalRTracing(
            string rollingLogFilePath,
            string consumerName,
            string applicationName)
        {
            LogLog.InternalDebugging = true;

            var log = LogManager.GetLogger(consumerName);
            var logger = (Logger) log.Logger;
            logger.Additivity = false;
            logger.Level = logger.Hierarchy.LevelMap[AllLevelName];
            
            var rollingFileAppender = rollingLogFileAppenderBuilder.Build(rollingLogFilePath, "signalr");
            logger.AddAppender(rollingFileAppender);

            //Signalr Tracing only logs out at debug level, so we just dump everything into the signalrtrace db
            const string createSignalRSprocName = "[usp_Create_SignalRTraceLog]";
            var adoNetAppender = adoAppenderBuilder.BuildForEventSource(Level.Debug, createSignalRSprocName);
            logger.AddAppender(adoNetAppender);
            
            logger.Repository.Configured = true;

            return log;
        }

        //All loggers used by consumers receive pre-formatted event payloads so we just need to map this into the logger
        //and do not need additional formatting of log message
        public ILog BuildForApplicationTracing(
            string rollingLogFilePath,
            string consumerName,
            string applicationName) 
        {
            LogLog.InternalDebugging = true;
            
            var log = LogManager.GetLogger(consumerName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;

            logger.Level = logger.Hierarchy.LevelMap[AllLevelName];
            
            const string rollingLogFileName = "application";
            var rollingFileAppender = rollingLogFileAppenderBuilder.Build(rollingLogFilePath, rollingLogFileName);
            logger.AddAppender(rollingFileAppender);
            
            var debugAdoAppender = adoAppenderBuilder.BuildForEventSource(Level.Debug, CreateInfoLogSprocName);
            logger.AddAppender(debugAdoAppender);

            var errorAdoAppender = adoAppenderBuilder.BuildForEventSource(Level.Error, CreateErrorSprocName);
            logger.AddAppender(errorAdoAppender);
            
            var eventLogAppender = windowEventLogAppenderBuilder.Build(applicationName);
            logger.AddAppender(eventLogAppender); 
            
            logger.Repository.Configured = true;

            return log;
        }
    }
}