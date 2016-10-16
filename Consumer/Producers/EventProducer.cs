namespace Consumer.Producers
{
    using System;

    using Adapters;

    using Logger;

    using CustomConfiguration;
    using Events;
    using Functions;
    using Queues;

    using Easy.Logger;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;

    public class EventProducer : IEventProducer
    {
        private readonly ILogInitializer logInitializer;
        private readonly ILogService logService;
        private readonly IEventQueue<TraceReceivedEvent> traceReceivedEventQueue;
        private readonly ITraceSessionManager traceSessionManager;
        private ILogger producerEasyLogger;
        private EventProducerConfigurationElement eventProducerConfiguration;
        private TraceEventSession traceEventSession;
        private Action<Exception> raiseErrorInParentThread;

        private const string Session = "session";

        public EventProducer(
            ILogService logService,
            IEventQueue<TraceReceivedEvent> traceReceivedEventQueue,
            ITraceSessionManager traceSessionManager,
            ILogInitializer logInitializer)
        {
            this.logService = logService;
            this.traceReceivedEventQueue = traceReceivedEventQueue;
            this.traceSessionManager = traceSessionManager;
            this.logInitializer = logInitializer;
        }

        public void Start(EventProducerConfigurationElement eventProducerConfigurationElement)
        {
            eventProducerConfiguration = eventProducerConfigurationElement;
            SetUpLoggers();

            try
            {
                traceEventSession = traceSessionManager.CreateTraceEventSession(eventProducerConfiguration.EventSource + Session,
                        eventProducerConfiguration.EventSource);

                SubscribeToDebugTraceEventStream();
                SubscribeToErrorTraceEventStream();

                traceEventSession.Source.Process();
            }
            catch (Exception exception)
            {
                producerEasyLogger.Error(
                    $"An exception has been raised in Start(). For Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}");
                raiseErrorInParentThread(exception);
            }
        }

        public void Stop()
        {
            traceSessionManager.DisposeTraceEventSession(
                eventProducerConfiguration.EventSource + Session,
                traceEventSession);
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }

        private void SubscribeToDebugTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventProducerConfiguration.EventSource, "debug");

            eventStream.Subscribe(
                traceEvent => AddDebugEventToQueue(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an debug event from the event stream. Event processing will now stop. Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}"),
                () => producerEasyLogger.Debug(
                    $"The debug event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void SubscribeToErrorTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventProducerConfiguration.EventSource, "error");

            eventStream.Subscribe(
                traceEvent => AddErrorEventToQueue(new TraceEventAdapter(traceEvent)),
                exception =>
                producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an error event from the event stream. Event processing will now stop. Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}"),
                () =>
                producerEasyLogger.Debug(
                    $"The error event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void SetUpLoggers()
        {
            if (eventProducerConfiguration.LogDebugTracesToDatabase)
            {
                logInitializer.InitializeForDebugDatabaseLogging(
                    eventProducerConfiguration.ApplicationName,
                    eventProducerConfiguration.LogPath);
            }
            else
            {
                logInitializer.InitializeForErrorDatabaseLogging(
                    eventProducerConfiguration.ApplicationName,
                    eventProducerConfiguration.LogPath);
            }

            producerEasyLogger = logService.GetLogger(GetType());
        }

        private void AddErrorEventToQueue(ITraceEventAdapter traceEventAdapter)
        {
            traceReceivedEventQueue.Add(
                new ErrorTraceReceivedEvent
                    {
                        ClientAplicationName = eventProducerConfiguration.ApplicationName,
                        TraceEvent = traceEventAdapter
                    });
        }

        private void AddDebugEventToQueue(ITraceEventAdapter traceEventAdapter)
        {
            traceReceivedEventQueue.Add(
                new DebugTraceReceivedEvent
                    {
                        ClientAplicationName = eventProducerConfiguration.ApplicationName,
                        TraceEvent = traceEventAdapter
                    });
        }
    }
}