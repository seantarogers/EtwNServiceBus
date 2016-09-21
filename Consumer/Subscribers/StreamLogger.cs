namespace Consumer.Subscribers
{
    using System;

    using Consumer.Constants;

    public class StreamLogger
    {

        private void DebugEventStreamCompleted(object source)
        {
            pfTracingAsiLogger.DebugFormat(source, HostConstants.DebugStreamCompleted);
        }

        private void ErrorEventStreamCompleted(object source)
        {
            pfTracingAsiLogger.DebugFormat(source, HostConstants.ErrorStreamCompleted);
        }

        private void DebugEventStreamError(object source, Exception exception)
        {
            pfTracingAsiLogger.ErrorFormat(source, HostConstants.DebugStreamException, exception);
            throw exception;
        }

        private void ErrorEventStreamError(object source, Exception exception)
        {
            pfTracingAsiLogger.ErrorFormat(source, HostConstants.ErrorStreamException, exception);
            throw exception;
        }

    }
}