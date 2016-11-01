namespace Consumer.Events
{
    using Adapters;

    public abstract class TraceReceivedEvent
    {
        public string ProducerName { get; set; }
        public string ClientAplicationName { get; set; }
        public ITraceEventAdapter TraceEvent { get; set; }
    }
}