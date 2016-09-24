namespace Consumer.Commands
{
    using Handlers;

    using Functions;

    public abstract class CreateLogCommand : ICommand
    {
        public string ClientApplicationName { get; set; }
        public EventPayload EventPayload { get; set; }
    }
}