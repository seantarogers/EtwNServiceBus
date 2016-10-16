namespace Consumer.Queues
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Events;

    public class EventQueue<TEvent> : IEventQueue<TEvent>
        where TEvent : TraceReceivedEvent
    {
        private static readonly BlockingCollection<TEvent> blockingCollection;

        static EventQueue()
        {
            const int queueCapacity = 2000;
            blockingCollection = new BlockingCollection<TEvent>(queueCapacity);
        }

        public void Add(TEvent tEvent)
        {
            blockingCollection.Add(tEvent);
        }

        public IEnumerable<TEvent> GetConsumingEnumerable() => blockingCollection.GetConsumingEnumerable();

        public void CompleteAdding()
        {
            blockingCollection.CompleteAdding();
        }
    }
}