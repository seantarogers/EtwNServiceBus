namespace Provider.EventSources
{
    public interface IBusEventSource
    {
        void DebugFormat(string debugMessage, params object[] parameters);

        void ErrorFormat(string errorMessage, params object[] parameters);
    }
}