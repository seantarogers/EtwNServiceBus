namespace Consumer.Commands.Handlers
{
    public interface ICommandExecutor
    {
        void Execute(string sprocName, CreateLogCommand command);
    }
}