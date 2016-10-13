namespace Consumer.Consumers
{
    using System;

    using Adapters;

    using Commands;
    using Commands.Handlers;

    using Queues;

    using Events;
    using Functions;

    using Easy.Logger;

    public class DebugEventConsumer : IEventConsumer
    {
        private readonly IContainerAdapter containerAdapter;
        private readonly IEventQueue<DebugTraceReceivedEvent> debugQueue;
        private readonly ILogService logService;
        private ILogger easyLogger;
        private Action<Exception> raiseErrorInParentThread;

        public DebugEventConsumer(
            IEventQueue<DebugTraceReceivedEvent> debugQueue,
            ILogService logService, 
            IContainerAdapter containerAdapter)
        {
            this.debugQueue = debugQueue;
            this.logService = logService;
            this.containerAdapter = containerAdapter;
        }

        public void Start()
        {
            easyLogger = logService.GetLogger(GetType());

            try
            {
                foreach (var debugEvent in debugQueue.GetConsumingEnumerable())
                {
                    SinkEvent(debugEvent);
                }
            }
            catch (Exception exception)
            {
                easyLogger.Error($"Exception raised whilst consuming from the DebugQueue. Details: {exception}");
                raiseErrorInParentThread(exception);
            }
        }

        private void SinkEvent(TraceReceivedEvent traceReceivedEvent)
        {
            using (containerAdapter.BeginLifetimeScope())
            {
                var sinkPayload = BuildPayload(traceReceivedEvent);
                if (!sinkPayload.IsValid)
                {
                    easyLogger.Error($"DebugEvent received without a valid payload from clientApplication: {traceReceivedEvent.ClientAplicationName}");
                    return;
                }

                LogDebugEventToRollingFile(sinkPayload, traceReceivedEvent.EasyLogger);
                LogDebugEventToDatabase(traceReceivedEvent, sinkPayload);
            }
        }

        private void LogDebugEventToDatabase(
            TraceReceivedEvent debugTraceReceivedEvent, 
            EventPayload sinkPayload)
        {
            var createInfoDebugLogCommandHandler = containerAdapter.GetInstance<IOneWayCommandHandler<CreateDebugLogCommand>>();
            createInfoDebugLogCommandHandler.Handle(new CreateDebugLogCommand
            {
                ClientApplicationName = debugTraceReceivedEvent.ClientAplicationName,
                EventPayload = sinkPayload
            });            
        }

        private EventPayload BuildPayload(TraceReceivedEvent debugTraceReceivedEvent)
        {
            var eventPayloadBuilder = containerAdapter.GetInstance<IEventPayloadBuilder>();
            var sinkPayload = eventPayloadBuilder.Build(debugTraceReceivedEvent.TraceEventAdapter);
            return sinkPayload;
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }

        private static void LogDebugEventToRollingFile(
            EventPayload eventPayload, 
            ILogger clientApplicationEasyLogger)
        {
            clientApplicationEasyLogger.DebugFormat("{0} {1} {2}", 
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource, eventPayload.Payload);
        }
    }
}