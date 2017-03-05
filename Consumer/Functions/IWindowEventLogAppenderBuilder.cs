using log4net.Appender;

namespace Consumer.Functions
{
    public interface IWindowEventLogAppenderBuilder
    {
        IAppender Build(string applicationName);
    }
}