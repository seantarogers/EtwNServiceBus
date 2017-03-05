using System.Diagnostics.Tracing;
using Infrastructure;

namespace Provider.EventSources
{
    [EventSource(Name = EventSourceConstants.ApplicationEventSource)]
    public sealed class ApplicationEventSource<TSource> : EventSource, IApplicationEventSource<TSource>
    {
        private readonly string eventSource;

        public class Keywords
        {
            public const EventKeywords DebugTracing = (EventKeywords)1;
            public const EventKeywords ErrorTracing = (EventKeywords)2;
        }

        public ApplicationEventSource()
        {
            //get source once and reuse
            eventSource = typeof(TSource).ToString();
        }
        
        [NonEvent]
        public void DebugFormat(string debugMessage, params object[] parameters)
        {
            Debug(eventSource, string.Format(debugMessage, parameters));
        }

        [NonEvent]
        public void ErrorFormat(string errorMessage, params object[] parameters)
        {
            Error(eventSource, string.Format(errorMessage, parameters));
        }

        [Event(EventSourceConstants.EventSourceDebug, Level = EventLevel.Informational, Keywords = Keywords.DebugTracing
            )]
        public void Debug(string source, string debugMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(EventSourceConstants.EventSourceDebug, source, debugMessage);
            }
        }

        [Event(EventSourceConstants.EventSourceError, Level = EventLevel.Error, Keywords = Keywords.ErrorTracing)]
        public void Error(string source, string errorMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(EventSourceConstants.EventSourceError, source, errorMessage);
            }
        }
    }
}