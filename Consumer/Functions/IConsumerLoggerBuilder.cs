using log4net;

namespace Consumer.Functions
{
    public interface IConsumerLoggerBuilder
    {
        ILog BuildForBusTracing(
            string rollingLogFilePath,
            string consumerName,
            string applicationName);

        ILog BuildForApplicationTracing(
            string rollingLogFilePath,
            string consumerName,
            string applicationName);

        ILog BuildForSignalRTracing(
          string rollingLogFilePath,
          string consumerName,
          string applicationName);
    }
}