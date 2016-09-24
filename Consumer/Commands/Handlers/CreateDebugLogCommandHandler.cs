namespace Consumer.Commands.Handlers
{
    public class CreateDebugLogCommandHandler : IOneWayCommandHandler<CreateDebugLogCommand>
    {
        private readonly ICommandExecutor commandExecutor;
        
        public CreateDebugLogCommandHandler(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public void Handle(CreateDebugLogCommand command)
        {
            const string sprocName = "[Etw].[usp_Create_InfoDebugLog]";
            commandExecutor.Execute(sprocName, command);
        }
    }
}