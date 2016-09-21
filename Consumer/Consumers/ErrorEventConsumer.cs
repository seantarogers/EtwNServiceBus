namespace Consumer.Consumers
{
    using System;

    using Easy.Logger;

    public class ErrorEventConsumer : IEventConsumer
    {
        private readonly IContainerAdapter containerAdapter;
        private readonly IErrorQueueAdapter errorQueue;
        private readonly IAsiLogger pfTracingAsiLogger;

        private Action<Exception> raiseErrorInParentThread;

        public ErrorEventConsumer(
            IErrorQueueAdapter errorQueue,
            IAsiLogger pfTracingAsiLogger, 
            IContainerAdapter containerAdapter)
        {
            this.errorQueue = errorQueue;
            this.pfTracingAsiLogger = pfTracingAsiLogger;
            this.containerAdapter = containerAdapter;
        }

        public void Start()
        {
            try
            {
                foreach (var errorTraceReceivedEvent in errorQueue.GetConsumingEnumerable())
                {
                    SinkEvent(errorTraceReceivedEvent);
                }
            }
            catch (Exception exception)
            {
                pfTracingAsiLogger.ErrorFormat(this, HostConstants.ErrorEventConsumerError, exception);
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
                    pfTracingAsiLogger.DebugFormat(this, HostConstants.ErrorEventWithoutPayload,
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
                containerAdapter.GetInstance<IOneWayAmbientUnitOfWorkCommandHandler<CreateErrorCreateLogCommand>>();
            commandHandler.Handle(new CreateErrorCreateLogCommand
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