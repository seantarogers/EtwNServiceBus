namespace Consumer.Logger {
    using System.Diagnostics;

    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Layout;

    public class LogBuilder
    {
        public static ILog Build(string loggerName, string levelName, string appenderName, string logFileName)
        {
            log4net.Util.LogLog.InternalDebugging = true;
            var log = LogManager.GetLogger(loggerName);
            var logger = (log4net.Repository.Hierarchy.Logger)log.Logger;
            logger.Additivity = false;
            logger.Level = logger.Hierarchy.LevelMap[levelName];

            var rollingFileAppender = CreateRollingFileAppender(appenderName, logFileName);
            logger.AddAppender(rollingFileAppender);

            var eventLogAppender = CreateEventLogAppender("ela", "test");
            logger.AddAppender(eventLogAppender);
            logger.Repository.Configured = true;

            return log;
        }

        private static IAppender CreateRollingFileAppender(string name, string fileName)
        {
            var appender = new RollingFileAppender
                               {
                                   Name = name,
                                   File = fileName,
                                   RollingStyle = RollingFileAppender.RollingMode.Date,
                                   MaxSizeRollBackups = 5,
                                   MaxFileSize = 31457280, // 30mb
                                   AppendToFile = true,
                                   ImmediateFlush = true,
                                   PreserveLogFileNameExtension = true,
                                   DatePattern = "yyyy-MM-dd'-application-all.log'",
                               };

            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.Debug };
            appender.AddFilter(levelMatchFilter);
            var layout = new PatternLayout { ConversionPattern = "%newline %level %message " };
                //received event already contains full trace
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            appender.StaticLogFileName = false;

            return appender;
        }

        private static IAppender CreateEventLogAppender(string name, string applicationName)
        {
            if (!EventLog.SourceExists("ETWNServiceBus"))
            {
                EventLog.CreateEventSource("ETWNServiceBus", "DebugEvents");                
            }

            var appender = new EventLogAppender
            {
                Name = name,
                LogName = "ETWNServiceBus",
                ApplicationName = applicationName,                
            };

            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.Debug };
            appender.AddFilter(levelMatchFilter);
            var layout = new PatternLayout { ConversionPattern = "%newline %level contents: %message" };
            //received event already contains full trace
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();

            return appender;
        }
    }
}