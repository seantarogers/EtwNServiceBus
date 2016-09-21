using Easy.Logger;

namespace PFTracing.Etw.Host
{
    public static class EasyLogger
    {
        public static void InitializeTypeReference()
        {
            // need to do this to statically  
            // because we only reference the easy.logger type in the app.configuration configuration
            // ReSharper disable once UnusedVariable
            var easyLogger = Log4NetService.Instance.GetLogger(typeof(Program));
        }
    }
}