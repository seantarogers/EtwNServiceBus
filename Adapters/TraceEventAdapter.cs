using System;
using Microsoft.Diagnostics.Tracing;

namespace PFTracing.Etw.Host.Adapters
{
    public class TraceEventAdapter : ITraceEventAdapter
    {
        private readonly TraceEvent traceEvent;

        public TraceEventAdapter(TraceEvent traceEvent)
        {
            this.traceEvent = traceEvent;
        }

        public string EventName => traceEvent.EventName;

        public string ProviderName => traceEvent.ProviderName;

        public DateTime TimeStamp => traceEvent.TimeStamp;

        public string[] PayloadNames => traceEvent.PayloadNames;

        public object PayloadValue(int index) => traceEvent.PayloadValue(index);

        public string PayloadString(int index) => traceEvent.PayloadString(index);

        public object PayloadByName(string payloadName) => traceEvent.PayloadByName(payloadName);
    }
}