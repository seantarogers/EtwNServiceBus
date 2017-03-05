using System.Diagnostics;

namespace PFTracing.Etw.Host.Adapters
{
    public class EventLogAdapter : IEventLogAdapter
    {
        public bool SourceExists(string sourceName)
        {
            return EventLog.SourceExists(sourceName);
        }

        public void CreateEventSource(string source, string logName)
        {
            EventLog.CreateEventSource(source, logName);
        }
    }
}