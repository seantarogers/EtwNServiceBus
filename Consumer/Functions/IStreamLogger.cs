namespace Consumer.Functions
{
    using System;

    public interface IStreamLogger
    {
        void DebugEventStreamCompleted(object source);

        void ErrorEventStreamCompleted(object source);

        void DebugEventStreamError(object source, Exception exception);

        void ErrorEventStreamError(object source, Exception exception);
    }
}