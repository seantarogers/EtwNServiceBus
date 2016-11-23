namespace Provider.EtwLogger
{
    public interface IEtwLogger
    {
        void ErrorFormat(object source, string errorMessage, params object[] parameters);
        void DebugFormat(object source, string debugMessage, params object[] parameters);
    }
}