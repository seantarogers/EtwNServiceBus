namespace Consumer.Extensions
{
    using System;

    using Functions;
    using static Functions.EventLevelType;

    public static class StringExtensions
    {
        public static EventLevelType ToEventLevel(this string eventName)
        {
            if (eventName == "debug")
            {
                return Debug;
            }

            if (eventName == "error")
            {
                return Error;
            }

            throw new ApplicationException($"Cannot convert event name to event level type for eventname: {eventName}");
        }
    }
}