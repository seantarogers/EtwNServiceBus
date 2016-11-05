namespace Consumer.Command.Handlers
{
    using Commands;

    public interface INonTransactionalCommandHandler<in TCommand> : ICommandHandler<TCommand> where TCommand : Command
    {
    }
}