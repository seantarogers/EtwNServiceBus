namespace Consumer.Queues
{
    using System.Collections.Generic;

    using Consumer.Events;

    public interface IEventQueue<TEvent>
        where TEvent : TraceReceivedEvent
    {
        void Add(TEvent tEvent);

        IEnumerable<TEvent> GetConsumingEnumerable();

        void CompleteAdding();
    }
}