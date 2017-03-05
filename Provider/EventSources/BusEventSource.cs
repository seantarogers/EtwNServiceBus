using System.Diagnostics.Tracing;
using Infrastructure;

namespace Provider.EventSources
{
    [EventSource(Name = EventSourceConstants.BusEventSource)]
    public sealed class BusEventSource : EventSource, IBusEventSource
    {
        private const string BusSource = "BusSource";

        public class Keywords
        {
            public const EventKeywords DebugTracing = (EventKeywords)1;
            public const EventKeywords ErrorTracing = (EventKeywords)2;
        }

        [NonEvent]
        public void DebugFormat(string debugMessage, params object[] parameters)
        {
            Debug(BusSource, string.Format(debugMessage, parameters));
        }

        [NonEvent]
        public void ErrorFormat(string errorMessage, params object[] parameters)
        {
            Error(BusSource, string.Format(errorMessage, parameters));
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

        [Event(EventSourceConstants.EventSourceError,
            Level = EventLevel.Error, 
            Keywords = Keywords.ErrorTracing)]
        public void Error(string source, string errorMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(EventSourceConstants.EventSourceError, source, errorMessage);
            }
        }
    }
}