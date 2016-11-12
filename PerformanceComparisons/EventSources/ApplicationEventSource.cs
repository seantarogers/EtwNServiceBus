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
        
        [Event(EventSourceConstants.EventSourceDebug, 
            Level = EventLevel.Informational, 
            Keywords = Keywords.DebugTracing)]
        public void Debug(string source, string debugMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(EventSourceConstants.EventSourceDebug, source, debugMessage);
            }
        }
    }
}