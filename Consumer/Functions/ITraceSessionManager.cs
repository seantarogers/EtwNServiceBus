using Microsoft.Diagnostics.Tracing.Session;

namespace Consumer.Functions
{
    public interface ITraceSessionManager
    {
        TraceEventSession CreateTraceEventSession(string sessionName, string eventSourceName);

        void DisposeTraceEventSession(string sessionName, TraceEventSession traceEventSession);        
    }
}