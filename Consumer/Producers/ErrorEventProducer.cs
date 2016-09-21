namespace Consumer.Producers
{
    using Adapters;
    using Events;
    using Queues;

    using Easy.Logger;

    public class ErrorEventProducer : IErrorEventProducer
    {
        private readonly IErrorQueue errorQueue;

        public ErrorEventProducer(IErrorQueue errorQueue)
        {
            this.errorQueue = errorQueue;
        }

        public void ReceiveNextErrorEvent(
            ITraceEventAdapter traceEventAdapter,
            ILogger easyLogger,
            string clientApplicationName)
        {
            errorQueue.Add(new ErrorTraceReceivedEvent
            {
                ClientAplicationName = clientApplicationName,
                TraceEventAdapter = traceEventAdapter,
                EasyLogger = easyLogger
            });
        }
    }
}