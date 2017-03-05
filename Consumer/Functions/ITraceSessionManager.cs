using Consumer.Adapters;

namespace Consumer.Functions
{
    public interface ITraceSessionManager
    {
        ITraceEventSessionAdapter CreateTraceEventSession(string sessionName, string eventSourceName);
        void DisposeTraceEventSession(string sessionName, ITraceEventSessionAdapter traceEventSession);
    }
}