namespace Consumer.Logger
{
    public interface ILogInitializer
    {
        void InitializeForErrorDatabaseLogging(string applicationName, string logFileName);

        void InitializeForDebugDatabaseLogging(string applicationName, string logFileName);
    }
}