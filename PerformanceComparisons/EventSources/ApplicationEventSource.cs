using System.Diagnostics.Tracing;
using Infrastructure;

namespace PerformanceComparisons.EventSources
{
    [EventSource(Name = EventSourceConstants.ApplicationEventSource)]
    public sealed class ApplicationEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords DebugTracing = (EventKeywords)1;
        }

        [Event(EventSourceConstants.EventSourceError, Level = EventLevel.Informational, Keywords = Keywords.DebugTracing)]
        public void Error(string source, string errorMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(EventSourceConstants.EventSourceError, source, errorMessage);
            }
        }
    }
}