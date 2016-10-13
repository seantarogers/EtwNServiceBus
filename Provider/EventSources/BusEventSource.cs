namespace Provider.EventSources
{
    using System.Diagnostics.Tracing;

    using Infrastructure;

    [EventSource(Name = EventSourceConstants.BusEventSource)]
    public sealed class BusEventSource : EventSource, IBusEventSource
    {
        public class Keywords
        {
            public const EventKeywords DebugTracing = (EventKeywords)1;
            public const EventKeywords ErrorTracing = (EventKeywords)2;
        }

        [NonEvent]
        public void DebugFormat(object source, string debugMessage, params object[] parameters)
        {
            var debug = string.Format(debugMessage, parameters);
            Debug(source.ToString(), debug);
        }

        [NonEvent]
        public void ErrorFormat(object source, string errorMessage, params object[] parameters)
        {
            var error = string.Format(errorMessage, parameters);
            Error(source.ToString(), error);
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