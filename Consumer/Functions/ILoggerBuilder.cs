namespace Consumer.Functions
{
    using log4net;

    public interface ILoggerBuilder
    {
        ILog BuildForBusLogging(string applicationName, string logFileName, string rollingLogFileName);

        ILog BuildForApplicationLogging(string applicationName, string logFileName, string fileName);
    }
}