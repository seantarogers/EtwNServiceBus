namespace Consumer.Events
{
    using Adapters;

    using Easy.Logger;

    public abstract class TraceReceivedEvent
    {
        public string ClientAplicationName { get; set; }
        public ITraceEventAdapter TraceEventAdapter { get; set; }
        public ILogger EasyLogger { get; set; }
    }
}