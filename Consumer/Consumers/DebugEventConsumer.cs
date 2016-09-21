namespace Consumer.Consumers
{
    using System;

    using Easy.Logger;

    public class DebugEventConsumer : IEventConsumer
    {
        private readonly IContainerAdapter containerAdapter;
        private readonly IDebugQueueAdapter debugQueue;
        private readonly IAsiLogger pfTracingAsiLogger;

        private Action<Exception> raiseErrorInParentThread;

        public DebugEventConsumer(
            IDebugQueueAdapter debugQueue,
            IAsiLogger pfTracingAsiLogger, 
            IContainerAdapter containerAdapter)
        {
            this.debugQueue = debugQueue;
            this.pfTracingAsiLogger = pfTracingAsiLogger;
            this.containerAdapter = containerAdapter;
        }

        public void Start()
        {
            try
            {
                foreach (var debugTraceReceivedEvent in debugQueue.GetConsumingEnumerable())
                {
                    SinkEvent(debugTraceReceivedEvent);
                }
            }
            catch (Exception exception)
            {
                pfTracingAsiLogger.ErrorFormat(this, HostConstants.DebugEventConsumerError, exception);
                raiseErrorInParentThread(exception);
            }
        }

        private void SinkEvent(TraceReceivedEvent debugTraceReceivedEvent)
        {
            using (containerAdapter.BeginLifetimeScope())
            {
                var sinkPayload = BuildPayload(debugTraceReceivedEvent);
                if (!sinkPayload.IsValid)
                {
                    pfTracingAsiLogger.DebugFormat(this, HostConstants.DebugEventWithoutPayload,
                        debugTraceReceivedEvent.ClientAplicationName);
                    return;
                }

                LogDebugEventToRollingFile(sinkPayload, debugTraceReceivedEvent.EasyLogger);
                LogDebugEventToDatabase(debugTraceReceivedEvent, sinkPayload);
            }
        }

        private void LogDebugEventToDatabase(TraceReceivedEvent debugTraceReceivedEvent, EventPayload sinkPayload)
        {
            var createInfoDebugLogCommandHandler = containerAdapter.GetInstance<IOneWayAmbientUnitOfWorkCommandHandler<CreateInfoDebugCreateLogCommand>>();
            createInfoDebugLogCommandHandler.Handle(new CreateInfoDebugCreateLogCommand
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

        private static void LogDebugEventToRollingFile(EventPayload eventPayload, ILogger clientApplicationEasyLogger)
        {
            clientApplicationEasyLogger.DebugFormat("{0} {1} {2}", 
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource, eventPayload.Payload);
        }
    }
}