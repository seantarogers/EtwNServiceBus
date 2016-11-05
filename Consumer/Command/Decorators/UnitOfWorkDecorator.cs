namespace Consumer.Command.Decorators
{
    namespace Consumer.Commands.Decorators
    {
        using System.Transactions;

        using Command.Commands;

        using global::Consumer.Command.Handlers;

        public class UnitOfWorkDecorator<TCommand> : ITransactionalCommandHandler<TCommand> where TCommand : Command
        {
            private readonly ITransactionalCommandHandler<TCommand> decoratedCommandHandler;

            public UnitOfWorkDecorator(ITransactionalCommandHandler<TCommand> decoratedCommandHandler)
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
}