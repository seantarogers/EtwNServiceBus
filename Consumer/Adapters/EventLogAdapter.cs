using System.Diagnostics;

namespace Consumer.Adapters
{
    public class EventLogAdapter : IEventLogAdapter
    {
        public bool SourceExists(string sourceName) => EventLog.SourceExists(sourceName);

        public void CreateEventSource(string source, string logName)
        {
            EventLog.CreateEventSource(source, logName);
        }
    }
}