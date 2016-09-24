namespace Consumer.Commands.Decorators
{
    using System.Transactions;

    using Handlers;

    public class OneWayAmbientUnitOfWorkDecorator<TCommand> 
        : IOneWayCommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IOneWayCommandHandler<TCommand> decoratedCommandHandler;

        public OneWayAmbientUnitOfWorkDecorator(IOneWayCommandHandler<TCommand> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Handle(TCommand command)
        {
            using (var transactionScope = new TransactionScope())
            {
                decoratedCommandHandler.Handle(command);
                transactionScope.Complete();
            }
        }
    }
}