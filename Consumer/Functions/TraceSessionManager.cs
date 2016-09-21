namespace Consumer.Functions
{
    using Consumer.Constants;

    using Microsoft.Diagnostics.Tracing.Session;

    public class TraceSessionManager : ITraceSessionManager
    {
        private readonly IAsiLogger asiLogger;
        private static readonly object ThisLock = new object();

        public TraceSessionManager(IAsiLogger asiLogger)
        {
            this.asiLogger = asiLogger;
        }

        public TraceEventSession CreateTraceEventSession(string sessionName, string eventSourceName)
        {
            lock (ThisLock)
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
            lock (ThisLock)
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
                    asiLogger.ErrorFormat(this, HostConstants.LostEventsThisSession, sessionName,
                        traceEventSession.EventsLost);
                }

                traceEventSession.Dispose();
            }
        }

        private void DisposeExistingSession(string sessionName)
        {
            asiLogger.DebugFormat(this, HostConstants.TracingSessionAlreadyExists, sessionName);
            var existingTraceEventSession = new TraceEventSession(sessionName);
            existingTraceEventSession.Dispose();
        }

        public bool TraceEventSessionIsActive(string sessionName)
        {
            return TraceEventSession.GetActiveSessionNames().Contains(sessionName);
        }
    }
}