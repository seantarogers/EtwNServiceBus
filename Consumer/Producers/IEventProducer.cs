namespace Consumer.Producers
{
    public interface IEventProducer
    {
        void ReceiveNextErrorEvent(ITraceEventAdapter traceEventAdapter);
        void ReceiveNextDebugEvent(ITraceEventAdapter traceEventAdapter);
    }
}