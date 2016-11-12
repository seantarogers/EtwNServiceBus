using System;

using Microsoft.Diagnostics.Tracing.Session;

namespace Consumer.Functions
{
    public class TraceSessionManager : ITraceSessionManager
    {
        private static readonly object thisLock = new object();
        
        public TraceEventSession CreateTraceEventSession(string sessionName, string eventSourceName)
        {
            lock (thisLock)
            {
                if (TraceEventSessionIsActive(sessionName))
                {
                    DisposeExistingSession(sessionName);
                }

                var traceEventSession = new TraceEventSession(sessionName, null);
                traceEventSession.EnableProvider(eventSourceName);
                return traceEventSession;
            }
        }

        public void DisposeTraceEventSession(string sessionName, TraceEventSession traceEventSession)
        {
            lock (thisLock)
            {
                if (traceEventSession == null)
                {
                    return;
                }

                if (!TraceEventSessionIsActive(sessionName))
                {
                    return;
                }

                if (traceEventSession.EventsLost > 0)
                {
                    Console.WriteLine($"Session {sessionName} is closing down. This session lost {traceEventSession.EventsLost} events.");
                }

                traceEventSession.Dispose();
            }
        }

        private static void DisposeExistingSession(string sessionName)
        {
            Console.WriteLine($"Tracing session {sessionName} already exists, will remove and create a new one.");

            var existingTraceEventSession = new TraceEventSession(sessionName);
            existingTraceEventSession.Dispose();
        }

        private static bool TraceEventSessionIsActive(string sessionName) => TraceEventSession.GetActiveSessionNames().Contains(sessionName);
    }
}