namespace Consumer.Functions
{
    using Consumer.Constants;

    using Easy.Logger;

    using Microsoft.Diagnostics.Tracing.Session;

    public class TraceSessionManager : ITraceSessionManager
    {
        private readonly ILogger easyLogger;
        private static readonly object thisLock = new object();

        public TraceSessionManager(ILogService logService)
        {
            easyLogger = logService.GetLogger(GetType());
        }

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
                    easyLogger.ErrorFormat(HostConstants.LostEventsThisSession, sessionName,
                        traceEventSession.EventsLost);
                }

                traceEventSession.Dispose();
            }
        }

        private void DisposeExistingSession(string sessionName)
        {
            easyLogger.DebugFormat(HostConstants.TracingSessionAlreadyExists, sessionName);
            var existingTraceEventSession = new TraceEventSession(sessionName);
            existingTraceEventSession.Dispose();
        }

        public bool TraceEventSessionIsActive(string sessionName)
        {
            return TraceEventSession.GetActiveSessionNames().Contains(sessionName);
        }
    }
}