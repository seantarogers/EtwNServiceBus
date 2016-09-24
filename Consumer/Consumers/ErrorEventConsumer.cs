namespace Consumer.Consumers
{
    using System;

    using Adapters;
    using Constants;

    using Consumer.Commands;
    using Consumer.Commands.Handlers;

    using Events;
    using Functions;
    using Queues;

    using Easy.Logger;

    public class ErrorEventConsumer : IEventConsumer
    {
        private readonly IContainerAdapter containerAdapter;
        private readonly IEventQueue<ErrorTraceReceivedEvent> errorQueue;
        private readonly ILogService logService;
        private  ILogger easyLogger;

        private Action<Exception> raiseErrorInParentThread;

        public ErrorEventConsumer(
            IEventQueue<ErrorTraceReceivedEvent> errorQueue,
            IContainerAdapter containerAdapter, 
            ILogService logService)
        {
            this.errorQueue = errorQueue;
            this.containerAdapter = containerAdapter;
            this.logService = logService;
        }

        public void Start()
        {
            easyLogger = logService.GetLogger(GetType());

            try
            {
                foreach (var errorTraceReceivedEvent in errorQueue.GetConsumingEnumerable())
                {
                    SinkEvent(errorTraceReceivedEvent);
                }
            }
            catch (Exception exception)
            {
                easyLogger.ErrorFormat(HostConstants.ErrorEventConsumerError, exception);
                raiseErrorInParentThread(exception);
            }
        }

        private void SinkEvent(TraceReceivedEvent errorTraceReceivedEvent)
        {
            using (containerAdapter.BeginLifetimeScope())
            {
                var eventPayload = CreateEventPayload(errorTraceReceivedEvent);
                if (!eventPayload.IsValid)
                {
                    easyLogger.DebugFormat(HostConstants.ErrorEventWithoutPayload,
                        errorTraceReceivedEvent.ClientAplicationName);
                    return;
                }

                LogErrorEventToRollingFile(eventPayload, errorTraceReceivedEvent.EasyLogger);
                LogErrorToDatabase(errorTraceReceivedEvent, eventPayload);
            }
        }

        private void LogErrorToDatabase(TraceReceivedEvent errorTraceReceivedEvent, EventPayload sinkPayload)
        {
            var commandHandler =
                containerAdapter.GetInstance<IOneWayCommandHandler<CreateErrorLogCommand>>();
            commandHandler.Handle(new CreateErrorLogCommand
            {
                ClientApplicationName = errorTraceReceivedEvent.ClientAplicationName,
                EventPayload = sinkPayload
            });
        }

        private EventPayload CreateEventPayload(TraceReceivedEvent errorTraceReceivedEvent)
        {
            var eventPayloadBuilder = containerAdapter.GetInstance<IEventPayloadBuilder>();
            return eventPayloadBuilder.Build(errorTraceReceivedEvent.TraceEventAdapter);            
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }

        private static void LogErrorEventToRollingFile(EventPayload eventPayload, ILogger clientApplicationEasyLogger)
        {
            clientApplicationEasyLogger.ErrorFormat("{0} {1} {2}",
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource, eventPayload.Payload);
        }
    }
}