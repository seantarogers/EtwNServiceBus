using log4net;
using Consumer.CustomConfiguration;

namespace Consumer.Functions
{
    public interface ILoggerBuilder
    {
        bool IsApplicable(EventType eventType);

        ILog Build(string rollingLogFilePath, string consumerName, string applicationName);
    }
}