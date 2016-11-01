﻿namespace Consumer.Extensions
{
    using System;

    using Functions;

    using static Consumer.Constants.HostConstants;

    using static Functions.EventLevelType;

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