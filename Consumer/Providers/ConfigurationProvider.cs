using Consumer.CustomConfiguration;

namespace Consumer.Providers
{
    public class ConfigurationProvider
    {
        public DeploymentLocationType DeploymentLocation { get; set; }
        public string PremiumFinanceAuditConnectionString { get; set; }
        public bool RunLog4NetInDebugMode { get; set; }
        public int BufferFlushIntervalInSeconds { get; set; }
        public int FirstLevelBufferSizeInMb { get; set; }
        public int SecondLevelBufferSizeInNumberOfEvents { get; set; }
    }
}