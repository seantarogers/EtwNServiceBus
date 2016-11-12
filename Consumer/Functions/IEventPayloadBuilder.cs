using Consumer.Adapters;

namespace Consumer.Functions
{
    public interface IEventPayloadBuilder
    {
        EventPayload Build(ITraceEventAdapter traceEventAdapter);
    }
}