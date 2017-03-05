using System.Data;
using Consumer.Providers;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Util.TypeConverters;

namespace Consumer.Functions
{
    public class AdoAppenderBuilder : IAdoAppenderBuilder
    {
        private readonly ConfigurationProvider configurationProvider;

        public AdoAppenderBuilder(ConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }
        
        public IAppender BuildForEventSource(Level level, string sprocName)
        {
            var rawLayoutConverter = new RawLayoutConverter();
            var logDateParameter = new AdoNetAppenderParameter
            {
                ParameterName = "@LogDate",
                DbType = DbType.DateTime,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout("%property{LogDate}"))
            };

            var loggerParameter = CreateLoggerParameter(rawLayoutConverter, "%property{Logger}");
            var logMessageParameter = CreateLogMessageParameter(rawLayoutConverter, "%message");
            var applicationNameParameter = CreateApplicationNameParameter(rawLayoutConverter, "%property{ApplicationName}");

            var adoNetAppender = CreateAdoNetAppender(sprocName);
            ConfigureAppender(level, adoNetAppender, logDateParameter, loggerParameter, logMessageParameter, applicationNameParameter);

            return adoNetAppender;
        }

        private static void ConfigureAppender(
            Level level,
            AdoNetAppender adoNetAppender,
            AdoNetAppenderParameter logDateParameter,
            AdoNetAppenderParameter loggerParameter,
            AdoNetAppenderParameter logMessageParameter,
            AdoNetAppenderParameter applicationNameParameter)
        {
            adoNetAppender.AddParameter(logDateParameter);
            adoNetAppender.AddParameter(loggerParameter);
            adoNetAppender.AddParameter(logMessageParameter);
            adoNetAppender.AddParameter(applicationNameParameter);

            var levelRangeFilter = new LevelRangeFilter { LevelMin = level, LevelMax = level };
            adoNetAppender.AddFilter(levelRangeFilter);

            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = level };
            adoNetAppender.AddFilter(levelMatchFilter);
            adoNetAppender.ActivateOptions();
        }

        private static AdoNetAppenderParameter CreateApplicationNameParameter(IConvertFrom rawLayoutConverter, string pattern)
        {
            return new AdoNetAppenderParameter
            {
                ParameterName = "@ApplicationName",
                DbType = DbType.String,
                Size = 100,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout(pattern))
            };
        }

        private static AdoNetAppenderParameter CreateLogMessageParameter(IConvertFrom rawLayoutConverter, string pattern)
        {
            return new AdoNetAppenderParameter
            {
                ParameterName = "@LogMessage",
                DbType = DbType.String,
                Size = 8000,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout(pattern))
            };
        }

        private static AdoNetAppenderParameter CreateLoggerParameter(IConvertFrom rawLayoutConverter, string pattern)
        {
            return new AdoNetAppenderParameter
            {
                ParameterName = "@Logger",
                DbType = DbType.String,
                Size = 255,
                Layout = (IRawLayout)rawLayoutConverter.ConvertFrom(new PatternLayout(pattern))
            };
        }

        private AdoNetAppender CreateAdoNetAppender(string sprocName)
        {
            const string connectionType = "System.Data.SqlClient.SqlConnection, System.Data, Version = 1.0.3300.0, "
                                          + "Culture = neutral, PublicKeyToken = b77a5c561934e089";
            return new AdoNetAppender
            {
                BufferSize = configurationProvider.LoggingBufferSize,
                CommandType = CommandType.StoredProcedure,
                ConnectionType = connectionType,
                ConnectionString = configurationProvider.PremiumFinanceAuditConnectionString,
                CommandText = sprocName
            };
        }
    }
}