namespace Consumer.Consumers
{
    using System;

    using Adapters;

    using Events;
    using Functions;
    using Queues;

    using Easy.Logger;

    using log4net;

    public class DebugEventConsumer : IEventConsumer
    {
        private readonly IContainerAdapter containerAdapter;
        private readonly IEventQueue<TraceReceivedEvent> traceReceivedEventQueue;
        private readonly ILogService logService;
        private  ILogger easyLogger;

        private Action<Exception> raiseErrorInParentThread;

        public DebugEventConsumer(
            IEventQueue<TraceReceivedEvent> traceReceivedEventQueue,
            IContainerAdapter containerAdapter, 
            ILogService logService)
        {
            this.traceReceivedEventQueue = traceReceivedEventQueue;
            this.containerAdapter = containerAdapter;
            this.logService = logService;
        }

        public void Start()
        {
            easyLogger = logService.GetLogger(GetType());

            try
            {
                foreach (var traceReceivedEvent in traceReceivedEventQueue.GetConsumingEnumerable())
                {
                    SinkEvent(traceReceivedEvent);
                }
            }
            catch (Exception exception)
            {
                easyLogger.Error($"Exception raised whilst consuming from the debug traceReceivedEventQueue. Details: {exception}");
                raiseErrorInParentThread(exception);
            }
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }

        private void SinkEvent(TraceReceivedEvent traceReceivedEvent)
        {
            using (containerAdapter.BeginLifetimeScope())
            {
                var eventPayload = CreateEventPayload(traceReceivedEvent);
                if (!eventPayload.IsValid)
                {
                    easyLogger.Debug($"Event received without a valid payload from clientApplication: {traceReceivedEvent.ClientAplicationName}");
                    return;
                }

                var clientApplicationLog = LogManager.GetLogger(traceReceivedEvent.ClientAplicationName);

                SetCustomAdoProperties(traceReceivedEvent);

                clientApplicationLog.DebugFormat(
                    "{0} {1} {2}",
                    eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    eventPayload.TraceSource,
                    eventPayload.Payload);                
            }
        }
        
        private static void SetCustomAdoProperties(TraceReceivedEvent traceReceivedEvent)
        {
            LogicalThreadContext.Properties["LogDate"] = traceReceivedEvent.TraceEvent.TimeStamp;
            LogicalThreadContext.Properties["ApplicationName"] = traceReceivedEvent.ClientAplicationName;
        }

        private EventPayload CreateEventPayload(TraceReceivedEvent errorTraceReceivedEvent)
        {
            var eventPayloadBuilder = containerAdapter.GetInstance<IEventPayloadBuilder>();
            return eventPayloadBuilder.Build(errorTraceReceivedEvent.TraceEvent);            
        }        
    }
}