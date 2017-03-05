namespace Consumer.Constants
{
    public static class ConsumerConstants
    {
        public const string DebugEvents = "Debug";
        public const string ErrorEvents = "Error";
        public const string DebugEventStreamException = "An exception was raised whilst consuming an debug event from the event stream. Event processing will now stop. Event Source: {0}. Details: {1}";
        public const string DebugEventStreamComplete = "The debug event stream has completed for source: {0}.";
        public const string ErrorEventStreamException = "An exception was raised whilst consuming an error event from the event stream.Event processing will now stop.Event Source: {0}, Details: {1}";
        public const string ErrorEventStreamComplete = "The error event stream has completed for source: {0}.";
        public const string EventConsumerException = "An exception has been raised in Start(). For Event Source: {0}, Details: {1}";
        public const string InvalidPayload = "Event received without a valid payload from Event Source: {0}";
        public const string FlushingAllBufferedAppenders = "About to flush all buffered appenders";
        public const string FlushedAllBufferedAppenders = "Flushed {0} buffered appenders";
        public const string FlushingException = "An exception was raised whilst flushing the buffered appenders. Details: {0}";
    }
}