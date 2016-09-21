namespace Consumer.Queues
{
    using System.Collections.Generic;

    using Consumer.Events;

    public interface IErrorQueue
    {
        void Add(ErrorTraceReceivedEvent debugTraceReceivedEvent);

        IEnumerable<ErrorTraceReceivedEvent> GetConsumingEnumerable();

        void CompleteAdding();
    }
}