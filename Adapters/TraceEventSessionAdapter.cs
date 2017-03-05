﻿using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace PFTracing.Etw.Host.Adapters
{
    public class TraceEventSessionAdapter : ITraceEventSessionAdapter
    {
        private readonly TraceEventSession traceEventSession;

        public TraceEventSessionAdapter(TraceEventSession traceEventSession)
        {
            this.traceEventSession = traceEventSession;
        }

        public IObservable<TraceEvent> Observe(string eventSourceName, string eventName)
        {
            return traceEventSession.Source.Dynamic.Observe(eventSourceName, eventName);
        }

        public bool Process()
        {
            return traceEventSession.Source.Process();
        }

        public int EventsLost => traceEventSession.EventsLost;
        
        public void Dispose()
        {
            traceEventSession?.Dispose();
        }
    }
}