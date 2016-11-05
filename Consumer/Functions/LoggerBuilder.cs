namespace Consumer.Functions {
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

    public class LoggerBuilder : ILoggerBuilder
    {
        private readonly ConfigurationProvider configurationProvider;

        private const string CreateErrorSprocName = "[dbo].[usp_Create_ErrorLog]";

        private const string LevelName = "ALL";

        public LoggerBuilder(ConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        public ILog BuildForBusLogging(
            string rollingLogFilePath,
            string applicationName,
            string rollingLogFileName)
        {
            LogLog.InternalDebugging = true;
            var log = LogManager.GetLogger(applicationName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            
            logger.Level = logger.Hierarchy.LevelMap[LevelName];

            var rollingFileAppender = CreateRollingFileAppender(applicationName, rollingLogFilePath, rollingLogFileName);
            logger.AddAppender(rollingFileAppender);

            // only log bus errors to sql, 
            // do not log bus debug statements to sql as it will be too noisy
            var adoNetAppender = CreateAdoAppender(Level.Error, CreateErrorSprocName);
            logger.AddAppender(adoNetAppender);

            //var eventLogAppender = CreateWindowsEventLogAppender(applicationName);
            //logger.AddAppender(eventLogAppender);
            //logger.Repository.Configured = true;

            return log;
        }

        public ILog BuildForApplicationLogging(
            string rollingLogFilePath,
            string applicationName, 
            string rollingLogFileName)
        {
            LogLog.InternalDebugging = true;

            var log = LogManager.GetLogger(applicationName);
            var logger = (Logger)log.Logger;
            logger.Additivity = false;
            
            logger.Level = logger.Hierarchy.LevelMap[LevelName];

            var debugRollingFileAppender = CreateRollingFileAppender(applicationName, rollingLogFilePath, rollingLogFileName);
            logger.AddAppender(debugRollingFileAppender);

            const string createInfoLogSprocName = "[dbo].[usp_Create_InfoDebugLog]";
            var debugAdoAppender = CreateAdoAppender(Level.Debug, createInfoLogSprocName);
            logger.AddAppender(debugAdoAppender);

            var errorAdoAppender = CreateAdoAppender(Level.Error, CreateErrorSprocName);
            logger.AddAppender(errorAdoAppender);

            //var eventLogAppender = CreateWindowsEventLogAppender(applicationName);
            //logger.AddAppender(eventLogAppender);
            //logger.Repository.Configured = true;

            return log;
        }

        private IAppender CreateAdoAppender(Level level, string sprocName)
        {
            var rawLayoutConverter = new RawLayoutConverter();

            var logDateParameter = new AdoNetAppenderParameter
            {
                ParameterName = "@LogDate",
                DbType = DbType.DateTime,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%property{LogDate}"))
            };

            var loggerParameter = new AdoNetAppenderParameter
            {
                ParameterName = "@Logger",
                DbType = DbType.String,
                Size = 255,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%property{Logger}"))
            };

            var logMessageParameter = new AdoNetAppenderParameter {
                ParameterName = "@LogMessage",
                DbType = DbType.String,
                Size = 8000,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%message"))
            };

            var applicationNameParameter = new AdoNetAppenderParameter
                                      {
                                          ParameterName = "@ApplicationName",
                                          DbType = DbType.String,
                                          Size = 100,
                                          Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%property{ApplicationName}"))
                                      };

            const string connectionType = "System.Data.SqlClient.SqlConnection, System.Data, Version = 1.0.3300.0, "
                                          + "Culture = neutral, PublicKeyToken = b77a5c561934e089";

            
            var adoNetAppender = new AdoNetAppender
            {
                BufferSize = 1, //keep this at one for the time being
                CommandType = CommandType.StoredProcedure,
                ConnectionType = connectionType,
                ConnectionString = configurationProvider.ConnectionString,
                CommandText = sprocName
            };

            adoNetAppender.AddParameter(logDateParameter);
            adoNetAppender.AddParameter(loggerParameter);
            adoNetAppender.AddParameter(logMessageParameter);
            adoNetAppender.AddParameter(applicationNameParameter);

            var levelRangeFilter = new LevelRangeFilter { LevelMin = level, LevelMax = level };
            adoNetAppender.AddFilter(levelRangeFilter);
            
            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = level };
            adoNetAppender.AddFilter(levelMatchFilter);
            adoNetAppender.ActivateOptions();

            return adoNetAppender;
        }

        private static IAppender CreateRollingFileAppender(
            string applicationName, 
            string rollingLogFilePath, 
            string rollingLogFileName)
        {
            var appender = new RollingFileAppender
            {
                Name = applicationName + "RollingFileAppender",
                File = rollingLogFilePath,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaxSizeRollBackups = 5,
                MaxFileSize = 31457280, // 30mb
                AppendToFile = true,
                ImmediateFlush = true,
                PreserveLogFileNameExtension = true,
                DatePattern = $"yyyy-MM-dd'-{rollingLogFileName}-all.log'",
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

            // only log errors to the windows event log, not debugs as these entries generate scom alerts
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