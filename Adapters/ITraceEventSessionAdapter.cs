using System;
using Microsoft.Diagnostics.Tracing;

namespace PFTracing.Etw.Host.Adapters
{
    public interface ITraceEventSessionAdapter : IDisposable
    {
        IObservable<TraceEvent> Observe(string eventSourceName, string eventName);
        int EventsLost { get; }
        bool Process();
    }
}