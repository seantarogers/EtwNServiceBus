namespace Consumer.Command.Handlers
{
    using System.Diagnostics;

    using Commands;
    using Providers;

    public class CreateWindowsEventLogCommandHandler : INonTransactionalCommandHandler<CreateWindowsEventLogCommand>
    {
        private readonly ConfigurationProvider configurationProvider;

        public CreateWindowsEventLogCommandHandler(ConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        public void Handle(CreateWindowsEventLogCommand command)
        {
            var logName = configurationProvider.ScomEventSource + "ErrorEvents";
            if (!EventLog.SourceExists(configurationProvider.ScomEventSource))
            {
                EventLog.CreateEventSource(configurationProvider.ScomEventSource, logName);
            }

            var eventLog = new EventLog(logName) { Source = configurationProvider.ScomEventSource };
            eventLog.WriteEntry(command.LogMessage);
        }
    }
}