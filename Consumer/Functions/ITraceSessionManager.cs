namespace Consumer.Functions
{
    using Microsoft.Diagnostics.Tracing.Session;

    public interface ITraceSessionManager
    {
        TraceEventSession CreateTraceEventSession(string sessionName, string eventSourceName);

        void DisposeTraceEventSession(string sessionName, TraceEventSession traceEventSession);

        bool TraceEventSessionIsActive(string sessionName);
    }
}