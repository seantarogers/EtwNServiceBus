namespace Consumer.Commands.Handlers
{
    using System;
    using System.Data;

    using Dapper;

    using Easy.Logger;

    using Providers;

    public class CommandExecutor : ICommandExecutor
    {
        private readonly IDapperConnectionFactory dapperConnectionFactory;
        private readonly ConfigurationProvider configurationProvider;
        private readonly ILogger easyLogger;

        public CommandExecutor(
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
                using (var dapperConnection = dapperConnectionFactory.CreateConnection(configurationProvider.ConnectionString))
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
                easyLogger.ErrorFormat("Error raised whilst logging an event to the database. Event Source: {0}, SprocName: {1}, Payload: {2}, Exception: {3}", command.ClientApplicationName,
                    sprocName, command.EventPayload, exception);
            }
        }
    }
}