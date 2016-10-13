namespace LoggerTests
{
    using System;
    using System.Diagnostics;

    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;

    using NUnit.Framework;

    [TestFixture]
    public class LoggingTests
    {
        public static ILog Build(string loggerName, string levelName, string logFileName)
        {
            log4net.Util.LogLog.InternalDebugging = true;
            var log = LogManager.GetLogger(loggerName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            logger.Level = logger.Hierarchy.LevelMap[levelName];

            var rollingFileAppender = CreateRollingFileAppender(loggerName + "rolling", logFileName);
            logger.AddAppender(rollingFileAppender);

            var eventLogAppender = CreateEventLogAppender(loggerName + "event", "test");
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
            try
            {
                if (!EventLog.SourceExists("ETWNServiceBus1"))
                {
                    EventLog.CreateEventSource("ETWNServiceBus1", "ETWNServiceBusDebugEvents");
                }

                var appender = new EventLogAppender
                                   {
                                       Name = name,
                                       LogName = "ETWNServiceBusDebugEvents",
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }


        [Test]
        public void DoesItWork()
        {
            // In order to set the level for a logger and add an appender reference you
            // can then use the following calls:
            var logger1 = Build("logger1", "ALL", @"C:\logs\etwnsb\");
            //var logger2 = CreateLogger("logger2", "ALL", "appenderName2", "logger2.log");

            //var logger1 = LogManager.GetLogger("logger1");
            //var logger2 = LogManager.GetLogger("logger2");
            
            logger1.Error("foo1");
            //logger2.Error("foo2");

        }
    }
}
