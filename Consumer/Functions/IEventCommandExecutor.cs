namespace Consumer.Functions
{
    using Consumer.Commands;

    public interface IEventCommandExecutor
    {
        void Execute(string sprocName, CreateLogCommand command);
    }
}