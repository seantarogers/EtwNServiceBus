namespace Provider.EtwLoggers
{
    using NServiceBus.Logging;

    using Provider.EventSources;

    public interface IEtwLogFactory : ILoggerFactory
    {
        void Initialize(IBusEventSource busEventSource);
    }
}