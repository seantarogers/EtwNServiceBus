using Consumer.CustomConfiguration;

namespace Consumer.Providers
{
    public class ConfigurationProvider
    {
        public DeploymentLocationType DeploymentLocation { get; set; }
        public string PremiumFinanceAuditConnectionString { get; set; }
        public int LoggingBufferSize { get; set; }
        public bool RunLog4NetInDebugMode { get; set; }
        public int BufferFlushIntervalInSeconds { get; set; }
    }
}