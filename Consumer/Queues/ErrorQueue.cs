namespace Consumer.Queues
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Events;

    public class ErrorQueue : IErrorQueue
    {
        private static readonly BlockingCollection<ErrorTraceReceivedEvent> ErrorBlockingCollection;

        static ErrorQueue()
        {
            const int queueCapacity = 2000;
            ErrorBlockingCollection = new BlockingCollection<ErrorTraceReceivedEvent>(queueCapacity);
        }

        public void Add(ErrorTraceReceivedEvent debugTraceReceivedEvent)
        {
            ErrorBlockingCollection.Add(debugTraceReceivedEvent);
        }

        public IEnumerable<ErrorTraceReceivedEvent> GetConsumingEnumerable()
        {
            return ErrorBlockingCollection.GetConsumingEnumerable();
        }

        public void CompleteAdding()
        {
            ErrorBlockingCollection.CompleteAdding();
        }
    }
}