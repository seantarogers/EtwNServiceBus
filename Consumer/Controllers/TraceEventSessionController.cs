namespace Consumer.Controllers
{
    using Adapters;
    using Providers;

    using log4net;

    using Microsoft.Diagnostics.Tracing.Session;

    public class TraceEventSessionController : ITraceEventSessionController
    {
        private readonly ILog logger;
        private readonly ConfigurationProvider configurationProvider;
        private static readonly object thisLock = new object();

        public TraceEventSessionController(ILog logger, ConfigurationProvider configurationProvider)
        {
            this.logger = logger;
            this.configurationProvider = configurationProvider;
        }

        public ITraceEventSessionAdapter CreateTraceEventSession(string sessionName, string eventSourceName)
        {
            lock (thisLock)
            {
                if (TraceEventSessionIsActive(sessionName))
                {
                    DisposeExistingSession(sessionName);
                }

                var traceEventSession = new TraceEventSession(sessionName, null)
                                            {
                                                BufferSizeMB = configurationProvider.FirstLevelBufferSizeInMb
                                            };
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