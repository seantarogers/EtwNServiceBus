namespace Consumer.Functions
{
    using Adapters;
    using Payloads;

    public interface IEventPayloadBuilder
    {
        EventPayload Build(ITraceEventAdapter traceEventAdapter);
    }
}