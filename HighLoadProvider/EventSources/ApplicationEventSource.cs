namespace HighLoadProvider.EventSources
{
    using System.Diagnostics.Tracing;

    using Infrastructure;

    [EventSource(Name = EventSourceConstants.ApplicationEventSource)]
    public sealed class ApplicationEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords DebugTracing = (EventKeywords)1;
        }

        [Event(EventSourceConstants.EventSourceDebug, Level = EventLevel.Informational, Keywords = Keywords.DebugTracing)]
        public void Debug(string source, string debugMessage)
        {
            WriteEvent(EventSourceConstants.EventSourceDebug, source, debugMessage);
        }
    }
}