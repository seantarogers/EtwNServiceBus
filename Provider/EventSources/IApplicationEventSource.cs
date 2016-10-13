﻿namespace Provider.EventSources
{
    public interface IApplicationEventSource
    {
        void DebugFormat(object source, string debugMessage, params object[] parameters);

        void ErrorFormat(object source, string errorMessage, params object[] parameters);
    }
}