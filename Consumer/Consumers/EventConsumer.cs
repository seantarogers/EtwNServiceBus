namespace Consumer.Consumers
{
    using System;

    using Adapters;
    using Constants;
    using CustomConfiguration;
    using Functions;
    using Producers;

    using log4net;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;

    public class EventConsumer : IEventConsumer
    {
        private readonly IEventPayloadBuilder eventPayloadBuilder;
        private readonly ILoggerBuilder loggerBuilder;
        private readonly ITraceSessionManager traceSessionManager;
        private EventConsumerConfigurationElement eventConsumerConfiguration;
        private TraceEventSession traceEventSession;
        private Action<Exception> raiseErrorInParentThread;
        private ILog eventSinklogger;

        private const string Session = "-Session";

        public EventConsumer(
            ITraceSessionManager traceSessionManager,
            ILoggerBuilder loggerBuilder, 
            IEventPayloadBuilder eventPayloadBuilder)
        {
            this.traceSessionManager = traceSessionManager;
            this.loggerBuilder = loggerBuilder;
            this.eventPayloadBuilder = eventPayloadBuilder;
        }

        public void Start(EventConsumerConfigurationElement eventConsumerConfigurationElement)
        {
            eventConsumerConfiguration = eventConsumerConfigurationElement;
            InitializerLoggers();

            try
            {
                CreateTraceEventSession();
                SubscribeToDebugTraceEventStream();
                SubscribeToErrorTraceEventStream();
                StartTraceEventSession();
            }
            catch (Exception exception)
            {
                //producerEasyLogger.Error(
                //    $"An exception has been raised in Start(). For Event Source: {eventConsumerConfiguration.EventSource}, Details: {exception}");
                raiseErrorInParentThread(exception);
            }
        }

        public void Stop()
        {
            traceSessionManager.DisposeTraceEventSession(
                eventConsumerConfiguration.EventSource + Session,
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
                eventConsumerConfiguration.EventSource + Session,
                eventConsumerConfiguration.EventSource);
        }

        private void SubscribeToDebugTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventConsumerConfiguration.EventSource, ConsumerConstants.DebugLevel);

            eventStream.Subscribe(
                traceEvent => SinkDebugEvent(new TraceEventAdapter(traceEvent)),
                exception => producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an debug event from the event stream. Event processing will now stop. Event Source: {eventConsumerConfiguration.EventSource}, Details: {exception}"),
                () => producerEasyLogger.Debug(
                    $"The debug event stream has completed for source: {eventConsumerConfiguration.EventSource}."));
        }

        private void SubscribeToErrorTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(eventConsumerConfiguration.EventSource, ConsumerConstants.ErrorLevel);

            eventStream.Subscribe(
                traceEvent => SinkErrorEvent(new TraceEventAdapter(traceEvent)),
                exception =>
                producerEasyLogger.Error(
                    $"An exception was raised whilst consuming an error event from the event stream. Event processing will now stop. Event Source: {eventConsumerConfiguration.EventSource}, Details: {exception}"),
                () =>
                producerEasyLogger.Debug(
                    $"The error event stream has completed for source: {eventConsumerConfiguration.EventSource}."));
        }

        private void InitializerLoggers()
        {
            if (eventConsumerConfiguration.LogDebugTracesToDatabase)
            {
                eventSinklogger = loggerBuilder.BuildForApplicationLogging(
                    eventConsumerConfiguration.RollingLogPath,
                    eventConsumerConfiguration.ApplicationName,
                    eventConsumerConfiguration.RollingLogFileName);
            }
            else
            {
                eventSinklogger = loggerBuilder.BuildForBusLogging(
                    eventConsumerConfiguration.RollingLogPath,
                    eventConsumerConfiguration.ApplicationName,
                    eventConsumerConfiguration.RollingLogFileName);
            }
        }

        private void SinkErrorEvent(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                //easyLogger.Debug($"Event received without a valid payload from clientApplication: {traceReceivedEvent.ClientAplicationName}");
                return;
            }
            
            SetCustomAdoAppenderProperties(traceEventAdapter);

            eventSinklogger.ErrorFormat(
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
            
            SetCustomAdoAppenderProperties(traceEventAdapter);

            eventSinklogger.DebugFormat(
                "{0} {1} {2}",
                eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                eventPayload.TraceSource,
                eventPayload.Payload);
        }

        private void SetCustomAdoAppenderProperties(ITraceEventAdapter traceEventAdapter)
        {
            LogicalThreadContext.Properties["Logger"] = eventConsumerConfiguration.Name;
            LogicalThreadContext.Properties["LogDate"] = traceEventAdapter.TimeStamp;
            LogicalThreadContext.Properties["ApplicationName"] = eventConsumerConfiguration.ApplicationName;
        }
    }
}