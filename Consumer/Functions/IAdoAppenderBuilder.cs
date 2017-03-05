using log4net.Appender;
using log4net.Core;

namespace Consumer.Functions
{
    public interface IAdoAppenderBuilder
    {
        IAppender BuildForEventSource(Level level, string sprocName);
    }
}