﻿using System;
using NServiceBus.Logging;
using Provider.EventSources;

namespace Provider.EtwLogger
{
    public class EtwLog : ILog
    {
        private readonly string name;
        private readonly IBusEventSource busEventSource;

        public bool IsDebugEnabled { get; }
        public bool IsInfoEnabled { get; }
        public bool IsWarnEnabled { get; }
        public bool IsErrorEnabled { get; }
        public bool IsFatalEnabled { get; }

        public EtwLog(string name, LogLevel level, IBusEventSource busEventSource)
        {
            this.name = name;
            this.busEventSource = busEventSource;
            IsDebugEnabled = LogLevel.Debug >= level;
            IsInfoEnabled = LogLevel.Info >= level;
            IsWarnEnabled = LogLevel.Warn >= level;
            IsErrorEnabled = LogLevel.Error >= level;
            IsFatalEnabled = LogLevel.Fatal >= level;
        }

        public void Debug(string message)
        {
            busEventSource.DebugFormat(FormatMessage(message));
        }

        public void Debug(string message, Exception exception)
        {
            busEventSource.DebugFormat(FormatMessage(message), exception.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            busEventSource.DebugFormat(FormatMessage(format), args);
        }

        public void Info(string message)
        {
            busEventSource.DebugFormat(FormatMessage(message));
        }

        public void Info(string message, Exception exception)
        {
            busEventSource.DebugFormat(FormatMessage(message), exception.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            busEventSource.DebugFormat(FormatMessage(format), args);
        }

        public void Warn(string message)
        {
            busEventSource.DebugFormat(FormatMessage(message));
        }

        public void Warn(string message, Exception exception)
        {
            busEventSource.DebugFormat(FormatMessage(message), exception.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            busEventSource.DebugFormat(FormatMessage(format), args);
        }

        public void Error(string message)
        {
            busEventSource.DebugFormat(FormatMessage(message));
        }

        public void Error(string message, Exception exception)
        {
            busEventSource.DebugFormat(FormatMessage(message), exception.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            busEventSource.ErrorFormat(FormatMessage(format), args);
        }

        public void Fatal(string message)
        {
            busEventSource.ErrorFormat(FormatMessage(message));
        }

        public void Fatal(string message, Exception exception)
        {
            busEventSource.ErrorFormat(FormatMessage(message), exception.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        { 
            busEventSource.ErrorFormat(FormatMessage(format), args);
        }

        private string FormatMessage(string message) => $"Name: {name}, Message: {message}";
    }
}