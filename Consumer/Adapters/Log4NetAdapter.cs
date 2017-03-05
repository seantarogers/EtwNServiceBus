using log4net;
using log4net.Core;

namespace Consumer.Adapters
{
    public sealed class Log4NetAdapter<T> : LogImpl
    {
        public Log4NetAdapter() : base(LogManager.GetLogger(typeof(T)).Logger) { }
    }
}