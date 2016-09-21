namespace Consumer.Adapters
{
    using System;

    using Microsoft.Diagnostics.Tracing;

    public class TraceEventAdapter : ITraceEventAdapter
    {
        private readonly TraceEvent traceEvent;

        public TraceEventAdapter(TraceEvent traceEvent)
        {
            this.traceEvent = traceEvent;
        }

        public string ProviderName => traceEvent.ProviderName;

        public DateTime TimeStamp => traceEvent.TimeStamp;

        public string[] PayloadNames => traceEvent.PayloadNames;

        public object PayloadValue(int index)
        {
            return traceEvent.PayloadValue(index);
        }

        public string PayloadString(int index)
        {
            return traceEvent.PayloadString(index);
        }

        public object PayloadByName(string payloadName)
        {
            return traceEvent.PayloadByName(payloadName);
        }
    }
}