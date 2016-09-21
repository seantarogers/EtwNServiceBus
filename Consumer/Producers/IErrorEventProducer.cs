namespace Consumer.Producers
{
    using Consumer.Adapters;

    using Easy.Logger;

    public interface IErrorEventProducer
    {
        void ReceiveNextErrorEvent(
            ITraceEventAdapter traceEventAdapter,
            ILogger easyLogger,
            string clientApplicationName);
    }
}