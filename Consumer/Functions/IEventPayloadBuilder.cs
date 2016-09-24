namespace Consumer.Functions
{
    using Adapters;

    public interface IEventPayloadBuilder
    {
        EventPayload Build(ITraceEventAdapter traceEventAdapter);
    }
}