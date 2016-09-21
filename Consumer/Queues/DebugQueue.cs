namespace Consumer.Queues
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Events;

    public class DebugQueue : IDebugQueue
    {
        private static readonly BlockingCollection<DebugTraceReceivedEvent> DebugBlockingCollection;

        static DebugQueue()
        {
            const int queueCapacity = 2000;
            DebugBlockingCollection = new BlockingCollection<DebugTraceReceivedEvent>(queueCapacity);
        }

        public void Add(DebugTraceReceivedEvent debugTraceReceivedEvent)
        {
            DebugBlockingCollection.Add(debugTraceReceivedEvent);
        }

        public IEnumerable<DebugTraceReceivedEvent> GetConsumingEnumerable()
        {
            return DebugBlockingCollection.GetConsumingEnumerable();
        }

        public void CompleteAdding()
        {
            DebugBlockingCollection.CompleteAdding();
        }
    }
}