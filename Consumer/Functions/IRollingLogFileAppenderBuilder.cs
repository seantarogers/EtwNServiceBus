using log4net.Appender;

namespace Consumer.Functions
{
    public interface IRollingLogFileAppenderBuilder
    {
        IAppender Build(
          string rollingLogFilePath,
          string rollingLogFileName,
          string conversionPattern = "%newline %level %message ");
    }
}