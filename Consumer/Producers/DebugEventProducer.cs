namespace Consumer.Producers
{
    using Adapters;

    using Events;
    using Queues;

    using Easy.Logger;

    public class DebugEventProducer : IDebugEventProducer
    {
        private readonly IDebugQueue debugQueue;

        public DebugEventProducer(IDebugQueue debugQueue)
        {
            this.debugQueue = debugQueue;
        }
        
        public void ReceiveNextDebugEvent(
            ITraceEventAdapter traceEventAdapter,
            ILogger easyLogger,
            string clientApplicationName)
        {
            debugQueue.Add(new DebugTraceReceivedEvent
            {
                ClientAplicationName = clientApplicationName,
                TraceEventAdapter = traceEventAdapter,
                EasyLogger = easyLogger
            });
        }
    }
}