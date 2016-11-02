﻿namespace Provider.EtwLoggers
{
    using System;

    using global::NServiceBus.Logging;

    using Provider.EventSources;

    public class EtwLogFactory : IEtwLogFactory
    {
        private readonly LogLevel level;
        private IBusEventSource thisBusEventSource;

        public EtwLogFactory(LogLevel level)
        {
            this.level = level;
        }

        public void Initialize(IBusEventSource busEventSource)
        {
            thisBusEventSource = busEventSource;
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            if (thisBusEventSource == null)
            {
                throw new ApplicationException("Bus EventSource Not Initialized");
            }

            return new EtwLog(name, level, thisBusEventSource);
        }
    }
}