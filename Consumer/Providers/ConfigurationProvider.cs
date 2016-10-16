namespace Consumer.Providers
{
    public class ConfigurationProvider
    {
        public string ConnectionString { get; set; }
        public string ScomEventSource { get; set; }

        public string WindowsEventLogName { get; set; }
    }
}