namespace Consumer.Command.Handlers
{
    using Consumer.Command.Commands;

    public interface ICommandHandler<in TCommand> where TCommand : Command
    {
        void Handle(TCommand command);
    }
}