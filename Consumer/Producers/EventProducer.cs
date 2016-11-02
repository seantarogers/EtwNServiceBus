namespace Consumer.Producers
{
    using System;

    using Adapters;

    using Constants;

    using CustomConfiguration;

    using Functions;

    using Easy.Logger;

    using log4net;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;

    public class EventProducer : IEventProducer
    {
        private readonly IEventPayloadBuilder eventPayloadBuilder;
        private readonly ILogInitializer logInitializer;
        private readonly ILogService logService;
        private readonly ITraceSessionManager traceSessionManager;
        private ILogger producerEasyLogger;
        private EventProducerConfigurationElement eventProducerConfiguration;
        private TraceEventSession traceEventSession;
        private Action<Exception> raiseErrorInParentThread;

        private const string Session = "-Session";

        public EventProducer(
            ILogService logService,
            ITraceSessionManager traceSessionManager,
            ILogInitializer logInitializer, 
            IEventPayloadBuilder eventPayloadBuilder)
        {
            this.logService = logService;
            this.traceSessionManager = traceSessionManager;
            this.logInitializer = logInitializer;
            this.eventPayloadBuilder = eventPayloadBuilder;
        }

        public void Start(EventProducerConfigurationElement eventProducerConfigurationElement)
        {
            eventProducerConfiguration = eventProducerConfigurationElement;
            InitializeLoggers();

            try
            {
                CreateTraceEventSession();
                SubscribeToDebugTraceEventStream();
                SubscribeToErrorTraceEventStream();
                StartTraceEventSession();
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

        private void StartTraceEventSession()
        {
            traceEventSession.Source.Process();
        }

        private void CreateTraceEventSession()
        {
            traceEventSession = traceSessionManager.CreateTraceEventSession(
                eventProducerConfiguration.EventSource + Session,
                eventProducerConfiguration.EventSource);
        }

        private void SubscribeToDebugTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventProducerConfiguration.EventSource, ConsumerConstants.DebugLevel);

            eventStream.Subscribe(
                traceEvent => SinkDebugEvent(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an debug event from the event stream. Event processing will now stop. Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}"),
                () => producerEasyLogger.Debug(
                    $"The debug event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void SubscribeToErrorTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventProducerConfiguration.EventSource, ConsumerConstants.ErrorLevel);

            eventStream.Subscribe(
                traceEvent => SinkErrorEvent(new TraceEventAdapter(traceEvent)),
                exception =>
                producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an error event from the event stream. Event processing will now stop. Event Source: {eventProducerConfiguration.EventSource}, Details: {exception}"),
                () =>
                producerEasyLogger.Debug(
                    $"The error event stream has completed for source: {eventProducerConfiguration.EventSource}."));
        }

        private void InitializeLoggers()
        {
            if (eventProducerConfiguration.LogDebugTracesToDatabase)
            {
                logInitializer.InitializeForDebugDatabaseLogging(
                    eventProducerConfiguration.RollingLogPath,
                    eventProducerConfiguration.ApplicationName,
                    eventProducerConfiguration.RollingLogFileName);
            }
            else
            {
                logInitializer.InitializeForErrorDatabaseLogging(
                    eventProducerConfiguration.RollingLogPath,
                    eventProducerConfiguration.ApplicationName,
                    eventProducerConfiguration.RollingLogFileName);
            }

            //producerEasyLogger = logService.GetLogger(GetType());
        }

        private void SinkErrorEvent(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                //easyLogger.Debug($"Event received without a valid payload from clientApplication: {traceReceivedEvent.ClientAplicationName}");
                return;
            }

            var clientApplicationLog = LogManager.GetLogger(eventProducerConfiguration.ApplicationName);
            SetCustomAdoProperties(traceEventAdapter);

            clientApplicationLog.ErrorFormat(
                "{0} {1} {2}",
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource,
                eventPayload.Payload);
        }

        private void SinkDebugEvent(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                //easyLogger.Debug($"Event received without a valid payload from clientApplication: {traceReceivedEvent.ClientAplicationName}");
                return;
            }

            var clientApplicationLog = LogManager.GetLogger(eventProducerConfiguration.ApplicationName);
            SetCustomAdoProperties(traceEventAdapter);

            clientApplicationLog.DebugFormat(
                "{0} {1} {2}",
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource,
                eventPayload.Payload);
        }

        private void SetCustomAdoProperties(ITraceEventAdapter traceEventAdapter)
        {
            LogicalThreadContext.Properties["Logger"] = eventProducerConfiguration.Name;
            LogicalThreadContext.Properties["LogDate"] = traceEventAdapter.TimeStamp;
            LogicalThreadContext.Properties["ApplicationName"] = eventProducerConfiguration.ApplicationName;
        }
    }
}