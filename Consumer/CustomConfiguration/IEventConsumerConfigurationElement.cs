namespace Consumer.CustomConfiguration
{
    public interface IEventConsumerConfigurationElement
    {
        string Name { get; }
        string EventSource { get; }
        string ApplicationName { get; }
        string RollingLogPath { get; }
        EventType TraceEventType { get; }
    }
}