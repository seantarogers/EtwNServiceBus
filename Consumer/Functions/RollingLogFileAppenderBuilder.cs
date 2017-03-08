using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace Consumer.Functions
{
    public class RollingLogFileAppenderBuilder : IRollingLogFileAppenderBuilder
    {
        public IAppender Build(
          string rollingLogFilePath,
          string rollingLogFileName,
          string conversionPattern = "%newline %level %message ")
        {
            const int thirtyMb = 31457280;
            var appender = new RollingFileAppender
            {
                File = rollingLogFilePath,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                MaxSizeRollBackups = 200,
                MaxFileSize = thirtyMb,
                AppendToFile = true,
                PreserveLogFileNameExtension = true,
                DatePattern = $"yyyy-MM-dd'-{rollingLogFileName}-all.log'",
            };

            var levelMatchFilter = new LevelMatchFilter { AcceptOnMatch = true, LevelToMatch = Level.All };
            appender.AddFilter(levelMatchFilter);

            var layout = new PatternLayout { ConversionPattern = conversionPattern };
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            appender.StaticLogFileName = false;

            return appender;
        }
    }
}