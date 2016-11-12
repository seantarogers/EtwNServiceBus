using NServiceBus.Logging;
using Provider.EventSources;

namespace Provider.EtwLoggers
{
    public interface IEtwLogFactory : ILoggerFactory
    {
        void Initialize(IBusEventSource busEventSource);
    }
}