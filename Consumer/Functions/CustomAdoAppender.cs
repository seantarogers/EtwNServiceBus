using System.Configuration;
using log4net.Appender;

namespace Consumer.Functions
{
    public class CustomAdoNetAppender : AdoNetAppender
    {
        private static string connectionString;
        private readonly object sync = new object();

        protected override string ResolveConnectionString(out string connectionStringContext)
        {
            SetConnectionString();
            connectionStringContext = connectionString;
            return connectionStringContext;
        }

        private void SetConnectionString()
        {
            lock (sync)
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString =
                        ConfigurationManager.ConnectionStrings["Logging"].ConnectionString;
                }
            }
        }
    }
}