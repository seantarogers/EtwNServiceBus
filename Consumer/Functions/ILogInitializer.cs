namespace Consumer.Functions
{
    public interface ILogInitializer
    {
        void InitializeForErrorDatabaseLogging(string applicationName, string logFileName, string rollingLogFileName);

        void InitializeForDebugDatabaseLogging(string applicationName, string logFileName, string fileName);
    }
}