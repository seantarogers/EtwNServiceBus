namespace Consumer.Producers
{
    using Adapters;

    using Easy.Logger;

    public interface IDebugEventProducer
    {
        void ReceiveNextDebugEvent(
            ITraceEventAdapter traceEventAdapter,
            ILogger easyLogger,
            string clientApplicationName);
    }
}