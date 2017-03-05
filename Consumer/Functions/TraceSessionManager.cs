using Consumer.Adapters;
using log4net;
using Microsoft.Diagnostics.Tracing.Session;

namespace Consumer.Functions
{
    public class TraceSessionManager : ITraceSessionManager
    {
        private readonly ILog logger;
        private static readonly object thisLock = new object();

        public TraceSessionManager(ILog logger)
        {
            this.logger = logger;
        }

        public ITraceEventSessionAdapter CreateTraceEventSession(string sessionName, string eventSourceName)
        {
            lock (thisLock)
            {
                if (TraceEventSessionIsActive(sessionName))
                {
                    DisposeExistingSession(sessionName);
                }

                var traceEventSession = new TraceEventSession(sessionName, null);
                traceEventSession.EnableProvider(eventSourceName);
                return new TraceEventSessionAdapter(traceEventSession);
            }
        }

        public void DisposeTraceEventSession(string sessionName, ITraceEventSessionAdapter traceEventSessionAdapter)
        {
            lock (thisLock)
            {
                if (traceEventSessionAdapter == null)
                {
                    return;
                }

                if (!TraceEventSessionIsActive(sessionName))
                {
                    return;
                }

                if (traceEventSessionAdapter.EventsLost > 0)
                {
                    logger.Error($"Session {sessionName} is closing down. This session lost {traceEventSessionAdapter.EventsLost} events.");
                }

                traceEventSessionAdapter.Dispose();
            }
        }

        private void DisposeExistingSession(string sessionName)
        {
            logger.Debug($"Tracing session {sessionName} already exists, will remove and create a new one.");

            var existingTraceEventSession = new TraceEventSession(sessionName);
            existingTraceEventSession.Dispose();
        }

        private bool TraceEventSessionIsActive(string sessionName) => TraceEventSession.GetActiveSessionNames().Contains(sessionName);
    }
}