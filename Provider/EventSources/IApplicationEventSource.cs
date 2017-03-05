namespace Provider.EventSources
{
    // ReSharper disable once UnusedTypeParameter
    public interface IApplicationEventSource<TSource>
    {
        void DebugFormat(string debugMessage, params object[] parameters);

        void ErrorFormat(string errorMessage, params object[] parameters);
    }
}