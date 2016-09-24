namespace Consumer.Commands.Handlers
{

    public interface IOneWayCommandHandler<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
}