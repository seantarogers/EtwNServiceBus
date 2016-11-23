using NServiceBus.Logging;
using Provider.EventSources;

namespace Provider.EtwLogger
{
    public interface IEtwLogFactory : ILoggerFactory
    {
        void Initialize(IBusEventSource busEventSource);
    }
}