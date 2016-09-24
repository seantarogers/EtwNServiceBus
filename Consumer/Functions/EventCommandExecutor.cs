namespace Consumer.Functions
{
    using System;
    using System.Data;

    using Commands;

    using Dapper;

    using Providers;

    using Easy.Logger;

    public class EventCommandExecutor : IEventCommandExecutor
    {
        private readonly IDapperConnectionFactory dapperConnectionFactory;
        private readonly ConfigurationProvider configurationProvider;
        private readonly ILogger easyLogger;

        public EventCommandExecutor(
            ConfigurationProvider configurationProvider, 
            IDapperConnectionFactory dapperConnectionFactory, 
            ILogService logService)
        {
            this.configurationProvider = configurationProvider;
            this.dapperConnectionFactory = dapperConnectionFactory;
            easyLogger = logService.GetLogger(GetType());
        }

        public void Execute(string sprocName, CreateLogCommand command)
        {
            try
            {
                using (
                    var dapperConnection =
                        dapperConnectionFactory.CreateConnection(configurationProvider.ConnectionString))
                {
                    dapperConnection.Open();
                    dapperConnection.Execute(sprocName, new
                    {
                        logDate = command.EventPayload.TraceDate,
                        Logger = command.EventPayload.TraceSource,
                        LogMessage = command.EventPayload.Payload,
                        ApplicationName = command.ClientApplicationName
                    }, CommandType.StoredProcedure);
                }
            }
            catch (Exception exception)
            {
                // maybe implement a retry policy on sql deadlocks here, but lets wait and see if it  is a problem first
                easyLogger.ErrorFormat("", command.ClientApplicationName,
                    sprocName, command.EventPayload, exception);
            }
        }
    }
}