namespace Consumer.Logger {
    using System.Data;
    using System.Diagnostics;

    using Providers;

    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using log4net.Util;

    public  class LogInitializer : ILogInitializer
    {
        private readonly ConfigurationProvider configurationProvider;

        public LogInitializer(ConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        public void InitializeForErrorDatabaseLogging(string logFileName, string applicationName)
        {
            LogLog.InternalDebugging = true;
            var log = LogManager.GetLogger(applicationName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;

            const string levelName = "ALL";
            logger.Level = logger.Hierarchy.LevelMap[levelName];

            var rollingFileAppender = CreateRollingFileAppender(applicationName, logFileName);
            logger.AddAppender(rollingFileAppender);

            // only log bus errors to sql, not debug as it is too noisy
            var adoNetAppender = CreateAdoAppender(Level.Error);
            logger.AddAppender(adoNetAppender);

            var eventLogAppender = CreateWindowsEventLogAppender(applicationName);
            logger.AddAppender(eventLogAppender);
            logger.Repository.Configured = true;

        }

        public void InitializeForDebugDatabaseLogging(string logFileName, string applicationName)
        {
            LogLog.InternalDebugging = true;
            var log = LogManager.GetLogger(applicationName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;

            const string levelName = "ALL";

            logger.Level = logger.Hierarchy.LevelMap[levelName];

            var debugRollingFileAppender = CreateRollingFileAppender(applicationName, logFileName);
            logger.AddAppender(debugRollingFileAppender);

            var adoNetAppender = CreateAdoAppender(Level.Debug);
            logger.AddAppender(adoNetAppender);

            var eventLogAppender = CreateWindowsEventLogAppender(applicationName);
            logger.AddAppender(eventLogAppender);
            logger.Repository.Configured = true;
        }

        private IAppender CreateAdoAppender(Level level)
        {
            var logDate = new AdoNetAppenderParameter {
                                                      ParameterName = "@LogDate",
                                                      DbType = DbType.DateTime
                                                  };


            var rawLayoutConverter = new RawLayoutConverter();
            var logMessage = new AdoNetAppenderParameter {
                ParameterName = "@LogMessage",
                DbType = DbType.String,
                Size = 8000,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%message"))
            };
            
            var applicationName = new AdoNetAppenderParameter {
                                                     ParameterName = "@ApplicationName",
                                                     DbType = DbType.String,
                                                     Size = 255
                                                 };

            const string connectionType = "System.Data.SqlClient.SqlConnection, System.Data, Version = 1.0.3300.0, Culture = neutral, PublicKeyToken = b77a5c561934e089";
            var adoNetAppender = new AdoNetAppender
                             {
                                 BufferSize = 1,
                                 CommandType = CommandType.StoredProcedure,
                                 ConnectionType = connectionType,
                                 ConnectionString = configurationProvider.ConnectionString,
                                 CommandText = "usp_Create_ErrorLog"
                             };
            
            adoNetAppender.AddParameter(logDate);
            adoNetAppender.AddParameter(logMessage);
            adoNetAppender.AddParameter(applicationName);
            
            adoNetAppender.ActivateOptions();


            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = level };
            adoNetAppender.AddFilter(levelMatchFilter);

            adoNetAppender.ActivateOptions();
            return adoNetAppender;
        }

        private static IAppender CreateRollingFileAppender(string loggerName, string fileName)
        {
            var appender = new RollingFileAppender
            {
                Name = loggerName + "RollingFileAppender",
                File = fileName,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaxSizeRollBackups = 5,
                MaxFileSize = 31457280, // 30mb
                AppendToFile = true,
                ImmediateFlush = true,
                PreserveLogFileNameExtension = true,
                DatePattern = "yyyy-MM-dd'-application-all.log'",
            };

            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.All };
            appender.AddFilter(levelMatchFilter);

            //received event already contains full trace so just populate message
            var layout = new PatternLayout { ConversionPattern = "%newline %level %message " };
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            appender.StaticLogFileName = false;

            return appender;
        }

        private IAppender CreateWindowsEventLogAppender(string applicationName)
        {
            if (!EventLog.SourceExists(configurationProvider.ScomEventSource))
            {
                EventLog.CreateEventSource(configurationProvider.ScomEventSource, configurationProvider.ScomEventSource + "debugEvents");
            }

            var appender = new EventLogAppender
                               {
                                   Name = applicationName + "WindowsEventLogAppender",
                                   LogName = configurationProvider.ScomEventSource + "debugEvents",
                                   ApplicationName = applicationName,
                               };

            // only log errors to the windows event log, not debugs as these entries generate scoms
            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.Error };
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