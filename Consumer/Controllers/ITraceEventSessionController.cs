using Consumer.Adapters;

namespace Consumer.Controllers
{

    public interface ITraceEventSessionController
    {
        ITraceEventSessionAdapter CreateTraceEventSession(string sessionName, string eventSourceName);
        void DisposeTraceEventSession(string sessionName, ITraceEventSessionAdapter traceEventSession);
    }
}