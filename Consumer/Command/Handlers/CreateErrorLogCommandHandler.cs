namespace Consumer.Command.Handlers
{
    using System.Data;

    using Commands;
    using Dapper;
    using Providers;

    public class CreateErrorLogCommandHandler : ITransactionalCommandHandler<CreateErrorLogCommand>
    {
        private readonly IDapperConnectionFactory dapperConnectionFactory;

        private readonly ConfigurationProvider configurationProvider;

        public CreateErrorLogCommandHandler(IDapperConnectionFactory dapperConnectionFactory, ConfigurationProvider configurationProvider)
        {
            this.dapperConnectionFactory = dapperConnectionFactory;
            this.configurationProvider = configurationProvider;
        }

        public void Handle(CreateErrorLogCommand command)
        {
            using (var dapperconnection = dapperConnectionFactory.CreateConnection(configurationProvider.ConnectionString))
            {
                dapperconnection.Open();

                const string sprocName = "[usp_Create_ErrorLog]";

                dapperconnection.Execute(
                    sprocName,
                    new { command.LogDate, command.Logger, command.LogMessage, command.ApplicationName },
                    CommandType.StoredProcedure);
            }
        }
    }
}