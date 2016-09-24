namespace Consumer.Functions
{
    using System;

    public class EventPayload
    {
        public string ProviderName { get; set; }
        public bool IsValid { get; set; }
        public string TraceSource { get; set; }
        public DateTime TraceDate { get; set; }
        public string Payload { get; set; }
    }
}