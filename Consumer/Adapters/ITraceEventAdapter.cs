namespace Consumer.Adapters
{
    using System;

    public interface ITraceEventAdapter
    {
        string ProviderName { get; }
        DateTime TimeStamp { get; }
        string[] PayloadNames { get; }
        object PayloadValue(int index);
        string PayloadString(int index);
        object PayloadByName(string payloadName);
    }
}