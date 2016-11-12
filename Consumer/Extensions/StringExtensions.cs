using System;
using Consumer.Functions;
using static Consumer.Constants.ConsumerConstants;
using static Consumer.Functions.EventLevelType;

namespace Consumer.Extensions
{
    public static class StringExtensions
    {
        public static EventLevelType ToEventLevel(this string eventName)
        {
            if (eventName == DebugLevel)
            {
                return Debug;
            }

            if (eventName == ErrorLevel)
            {
                return Error;
            }

            throw new ApplicationException($"Cannot convert event name to event level type for eventname: {eventName}");
        }
    }
}