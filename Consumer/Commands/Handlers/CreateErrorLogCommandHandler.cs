namespace Consumer.Commands.Handlers
{
    public class CreateErrorLogCommandHandler : IOneWayCommandHandler<CreateErrorLogCommand>
    {
        private readonly ICommandExecutor commandExecutor;

        public CreateErrorLogCommandHandler(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public void Handle(CreateErrorLogCommand command)
        {
            const string sprocName = "[Etw].[usp_Create_ErrorLog]";
            commandExecutor.Execute(sprocName, command);
        }
    }
}