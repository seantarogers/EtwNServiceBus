namespace Consumer.Constants
{
    public class HostConstants
    {
        public static class EventTypes
        {
            public const string DebugEvents = "Debug";
            public const string ErrorEvents = "Error";
        }

        public static class Applications
        {
            public const string PfArApi = "PfArApi";
            public const string PfArHost = "PfArHost";
            public const string PfIdentity = "PfIdentity";
        }

        public const string DebugEventWithoutPayload =
            "DebugEvent received without a valid payload from clientApplication: {0}";
        public const string DebugEventConsumerError = "Exception raised whilst consuming from the DebugQueue.Details: {0}";

        public const string ErrorEventWithoutPayload =
           "ErrorEvent received without a valid payload from clientApplication: {0}";
        public const string ErrorEventConsumerError = "Exception raised whilst consuming from the ErrorQueue.Details: {0}";

        public const string DebugBusEventWithoutPayload =
            "DebugBusEvent received without a valid payload from clientApplication: {0}";
        public const string DebugBusEventConsumerError = "Exception raised whilst consuming from the DebugBusQueue.Details: {0}";

        public const string TracingSessionAlreadyExists =
            "Tracing session {0} already exists, will remove and create a new one.";

        public const string LostEventsThisSession = "Session {0} is closing down. This session lost {1} events.";

        public const string DebugStreamCompleted = "The debug event stream has completed for source: {0}.";
        public const string ErrorStreamCompleted = "The error event stream has completed for source: {0}.";

        public const string DebugStreamException =
            "An exception was raised whilst consuming a debug event from the debug event stream. Event processing will now stop. Source: {0}, Exception Details: {1}";

        public const string ErrorStreamException =
            "An exception was raised whilst consuming an error event from the error event stream. Event processing will now stop. Source: {0}, Exception Details: {1}";
    }
}