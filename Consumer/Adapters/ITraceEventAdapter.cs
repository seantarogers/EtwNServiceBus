using System;

namespace Consumer.Adapters
{
    public interface ITraceEventAdapter
    {
        string EventName { get; }
        string ProviderName { get; }
        DateTime TimeStamp { get; }
        string[] PayloadNames { get; }
        object PayloadValue(int index);
        string PayloadString(int index);
        object PayloadByName(string payloadName);        
    }
}