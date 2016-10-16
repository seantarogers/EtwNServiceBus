namespace Consumer.Events
{
    using Adapters;

    public abstract class TraceReceivedEvent
    {
        public string ClientAplicationName { get; set; }
        public ITraceEventAdapter TraceEvent { get; set; }
    }
}