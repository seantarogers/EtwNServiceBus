namespace Consumer.Queues
{
    using System.Collections.Generic;

    using Consumer.Events;

    public interface IDebugQueue
    {
        void Add(DebugTraceReceivedEvent debugTraceReceivedEvent);

        IEnumerable<DebugTraceReceivedEvent> GetConsumingEnumerable();

        void CompleteAdding();
    }
}