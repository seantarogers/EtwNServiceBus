namespace Consumer.Command.Handlers
{
    using System.Data;

    using Commands;
    using Dapper;
    using Providers;

    public class CreateDebugLogCommandHandler : ITransactionalCommandHandler<CreateDebugLogCommand>
    {
        private readonly IDapperConnectionFactory dapperConnectionFactory;

        private readonly ConfigurationProvider configurationProvider;

        public CreateDebugLogCommandHandler(IDapperConnectionFactory dapperConnectionFactory, ConfigurationProvider configurationProvider)
        {
            this.dapperConnectionFactory = dapperConnectionFactory;
            this.configurationProvider = configurationProvider;
        }

        public void Handle(CreateDebugLogCommand command)
        {
            using (var dapperconnection = dapperConnectionFactory.CreateConnection(configurationProvider.ConnectionString))
            {
                dapperconnection.Open();

                const string sprocName = "[usp_Create_InfoDebugLog]";

                dapperconnection.Execute(
                    sprocName,
                    new { command.LogDate, command.Logger, command.LogMessage, command.ApplicationName },
                    CommandType.StoredProcedure);
            }
        }
    }
}