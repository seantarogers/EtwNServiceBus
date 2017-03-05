using System;
using Microsoft.Diagnostics.Tracing;

namespace Consumer.Adapters
{
    public interface ITraceEventSessionAdapter
    {
        IObservable<TraceEvent> Observe(string eventSourceName, string eventName);

        bool Process();

        int EventsLost { get; }

        void Dispose();
    }
}