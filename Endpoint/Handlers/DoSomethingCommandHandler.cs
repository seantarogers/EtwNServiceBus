using System;
using System.Threading.Tasks;
using Infrastructure.Commands;
using NServiceBus;

namespace Endpoint.Handlers
{
    public class DoSomethingCommandHandler : IHandleMessages<DoSomethingCommand>
    {
        public Task Handle(DoSomethingCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine("Receiving DoSomethingCommand");
            return Task.CompletedTask;
        }
    }
}