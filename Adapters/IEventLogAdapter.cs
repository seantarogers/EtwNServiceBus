namespace PFTracing.Etw.Host.Adapters
{
    public interface IEventLogAdapter
    {
        bool SourceExists(string sourceName);
        void CreateEventSource(string source, string logName);
    }
}