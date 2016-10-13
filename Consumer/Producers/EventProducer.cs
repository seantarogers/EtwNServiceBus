namespace Consumer.Producers
{
    using System;

    using Adapters;

    using Consumer.Logger;

    using CustomConfiguration;
    using Events;
    using Functions;
    using Queues;

    using Easy.Logger;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;

    public class EventProducer : IEventProducer
    {
        private readonly ILogService logService;
        private readonly IEventQueue<DebugTraceReceivedEvent> debugQueue;
        private readonly IEventQueue<ErrorTraceReceivedEvent> errorQueue;
        
        private readonly ITraceSessionManager traceSessionManager;

        private ILogger applicationSourceEasyLogger;
        private ILogger producerEasyLogger;
        private EventProducerConfigurationElement eventProducerConfiguration;
        private TraceEventSession traceEventSession;
        private Action<Exception> raiseErrorInParentThread;

        private const string Session = "session";

        public EventProducer(
            ILogService logService,
            IEventQueue<DebugTraceReceivedEvent> debugQueue, 
            IEventQueue<ErrorTraceReceivedEvent> errorQueue, 
            ITraceSessionManager traceSessionManager)
        {
            this.logService = logService;
            this.debugQueue = debugQueue;
            this.errorQueue = errorQueue;
            this.traceSessionManager = traceSessionManager;
        }

        public void Start(EventProducerConfigurationElement eventProducerConfigurationElement)
        {
            eventProducerConfiguration = eventProducerConfigurationElement;
            SetUpEasyLoggers();

            try
            {
                traceEventSession = traceSessionManager.CreateTraceEventSession(
                    eventProducerConfiguration.EventSource + Session,
                    eventProducerConfiguration.EventSource);

                SubscribeToDebugEventStream();
                SubscribeToErrorEventStream();

                traceEventSession.Source.Process();
            }
            catch (Exception exception)
            {
                producerEasyLogger.Error($"An exception has been raised in Start(). For Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}");
                raiseErrorInParentThread(exception);
            }
        }

        public void Stop()
        {
            traceSessionManager.DisposeTraceEventSession(
                eventProducerConfiguration.EventSource + Session, traceEventSession);
        }

        private void SetUpEasyLoggers()
        {
            //LogBuilder.Build(eventProducerConfiguration.EasyLoggerName, "DEBUG")
            applicationSourceEasyLogger = logService.GetLogger(eventProducerConfiguration.EasyLoggerName);
            producerEasyLogger = logService.GetLogger(GetType());
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }
        
        private void SubscribeToErrorEventStream()
        {
            var errorEventStream = traceEventSession.Source.Dynamic.Observe(
                eventProducerConfiguration.EventSource,
                "Error");

            errorEventStream.Subscribe(
                traceEvent => AddErrorEventToQueue(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.Error($"An exception was raised whilst consuming an error event from the error event stream.Event processing will now stop.Source: {eventProducerConfiguration.EventSource}, Exception Details: {exception}"),
                () => producerEasyLogger.Debug($"The error event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void SubscribeToDebugEventStream()
        {
            var debugEventStream = traceEventSession.Source.Dynamic.Observe(
                eventProducerConfiguration.EventSource,
                "Debug");

            debugEventStream.Subscribe(
                traceEvent => AddDebugEventToQueue(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.Error($"An exception was raised whilst consuming a debug event from the debug event stream. Event processing will now stop. Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}"),
                () => producerEasyLogger.Debug($"The debug event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void AddErrorEventToQueue(ITraceEventAdapter traceEventAdapter)
        {
            errorQueue.Add(
                new ErrorTraceReceivedEvent
                    {
                        ClientAplicationName = eventProducerConfiguration.ApplicationName,
                        TraceEventAdapter = traceEventAdapter,
                        EasyLogger = applicationSourceEasyLogger
                    });
        }

        private void AddDebugEventToQueue(ITraceEventAdapter traceEventAdapter)
        {
            debugQueue.Add(
                new DebugTraceReceivedEvent
                    {
                        ClientAplicationName = eventProducerConfiguration.ApplicationName,
                        TraceEventAdapter = traceEventAdapter,
                        EasyLogger = applicationSourceEasyLogger
                    });
        }
    }
}