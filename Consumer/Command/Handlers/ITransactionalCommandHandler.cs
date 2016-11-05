namespace Consumer.Command.Handlers
{
    using Commands;

    public interface ITransactionalCommandHandler<in TCommand> : ICommandHandler<TCommand> where TCommand : Command
    {
    }
}