namespace Consumer.Producers
{
    using System;

    using Adapters;
    using Constants;
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
                producerEasyLogger.ErrorFormat( "An exception has been raised in Start(). For Source: {0} Details: {1}",
                    eventProducerConfiguration.EventSource, exception);
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
                HostConstants.EventTypes.ErrorEvents);

            errorEventStream.Subscribe(
                traceEvent => AddErrorEventToQueue(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.ErrorFormat(HostConstants.ErrorStreamException, eventProducerConfiguration.EventSource, exception),
                () => producerEasyLogger.DebugFormat(HostConstants.ErrorStreamCompleted, eventProducerConfiguration.EventSource));
        }

        private void SubscribeToDebugEventStream()
        {
            var debugEventStream = traceEventSession.Source.Dynamic.Observe(
                eventProducerConfiguration.EventSource,
                HostConstants.EventTypes.DebugEvents);

            debugEventStream.Subscribe(
                traceEvent => AddDebugEventToQueue(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.ErrorFormat(HostConstants.DebugStreamException, eventProducerConfiguration.EventSource, exception),
                () => producerEasyLogger.DebugFormat(HostConstants.DebugStreamCompleted, eventProducerConfiguration.EventSource));
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