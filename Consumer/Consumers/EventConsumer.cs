using System;
using System.Collections.Generic;
using Consumer.Adapters;
using Consumer.Functions;
using log4net;
using static Consumer.Constants.ConsumerConstants;
using static Consumer.CustomConfiguration.EventType;
using Consumer.CustomConfiguration;
using Consumer.Controllers;

namespace Consumer.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly IEventPayloadBuilder eventPayloadBuilder;
        private readonly IConsumerLoggerBuilder consumerLoggerBuilder;
        private readonly ITraceEventSessionController traceEventSessionController;
        private readonly ILog logger;

        private ILog eventConsumerLogger;
        private IEventConsumerConfigurationElement eventConsumerConfiguration;
        private ITraceEventSessionAdapter traceEventSession;
        private Action<Exception> raiseErrorInParentThread;
        private Dictionary<EventType, Func<ILog>> consumerLoggerMapper;

        private const string YyyyMmDdHhMmSs = "{0:yyyy-MM-dd HH:mm:ss} {1} {2}";
        private const string Session = "-Session";

        public string Name => eventConsumerConfiguration.Name;

        public EventConsumer(
            ITraceEventSessionController traceEventSessionController,
            IConsumerLoggerBuilder consumerLoggerBuilder,
            IEventPayloadBuilder eventPayloadBuilder,
            ILog logger)
        {
            this.traceEventSessionController = traceEventSessionController;
            this.consumerLoggerBuilder = consumerLoggerBuilder;
            this.eventPayloadBuilder = eventPayloadBuilder;
            this.logger = logger;
            InitializeConsumerLoggerMapper();
        }

        public void Start(IEventConsumerConfigurationElement eventConsumerConfigurationElement, Action<Exception> onError)
        {
            eventConsumerConfiguration = eventConsumerConfigurationElement;
            raiseErrorInParentThread = onError;

            eventConsumerLogger = GetConfiguredLoggerForThisEventConsumer();

            try
            {
                CreateTraceEventSession();
                SubscribeToDebugTraceEventStream();
                SubscribeToErrorTraceEventStream();

                StartTraceEventSession();
            }
            catch (Exception exception)
            {
                logger.ErrorFormat(EventConsumerException, eventConsumerConfiguration.EventSource, exception);
                raiseErrorInParentThread(exception);
            }
        }

        public void Stop()
        {
            traceEventSessionController.DisposeTraceEventSession(eventConsumerConfiguration.EventSource + Session, traceEventSession);
        }

        private ILog GetConfiguredLoggerForThisEventConsumer() => consumerLoggerMapper[eventConsumerConfiguration.TraceEventType].Invoke();

        private void StartTraceEventSession()
        {
            traceEventSession.Process();
        }

        private void CreateTraceEventSession()
        {
            traceEventSession = traceEventSessionController.CreateTraceEventSession(eventConsumerConfiguration.EventSource + Session, eventConsumerConfiguration.EventSource);
        }

        private void SubscribeToDebugTraceEventStream()
        {
            var debugEventStream = traceEventSession.Observe(eventConsumerConfiguration.EventSource, DebugEvents);

            debugEventStream.Subscribe(
                traceEvent => DebugEventSink(new TraceEventAdapter(traceEvent)),
                exception => logger.ErrorFormat(DebugEventStreamException, eventConsumerConfiguration.EventSource, exception),
                () => logger.ErrorFormat(DebugEventStreamComplete, eventConsumerConfiguration.EventSource));
        }

        private void SubscribeToErrorTraceEventStream()
        {
            var errorEventStream = traceEventSession.Observe(eventConsumerConfiguration.EventSource, ErrorEvents);

            errorEventStream.Subscribe(
                traceEvent => ErrorEventSink(new TraceEventAdapter(traceEvent)),
                exception => logger.ErrorFormat(ErrorEventStreamException, eventConsumerConfiguration.EventSource, exception),
                () => logger.ErrorFormat(ErrorEventStreamComplete, eventConsumerConfiguration.EventSource));
        }

        private void ErrorEventSink(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                logger.ErrorFormat(InvalidPayload, traceEventAdapter.ProviderName);
                return;
            }

            SetCustomAdoPropertiesToCurrentThread(traceEventAdapter);
            eventConsumerLogger.ErrorFormat(YyyyMmDdHhMmSs, eventPayload.TraceDate, eventPayload.TraceSource, eventPayload.Payload);
        }

        private void DebugEventSink(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                logger.ErrorFormat(InvalidPayload, traceEventAdapter.ProviderName);
                return;
            }

            SetCustomAdoPropertiesToCurrentThread(traceEventAdapter);
            eventConsumerLogger.DebugFormat(YyyyMmDdHhMmSs, eventPayload.TraceDate, eventPayload.TraceSource, eventPayload.Payload);
        }

        private void SetCustomAdoPropertiesToCurrentThread(ITraceEventAdapter traceEventAdapter)
        {
            LogicalThreadContext.Properties["Logger"] = eventConsumerConfiguration.Name;
            LogicalThreadContext.Properties["LogDate"] = traceEventAdapter.TimeStamp;
            LogicalThreadContext.Properties["ApplicationName"] = eventConsumerConfiguration.ApplicationName;
        }

        private void InitializeConsumerLoggerMapper()
        {
            consumerLoggerMapper = new Dictionary<EventType, Func<ILog>>
            {
                {
                    Application, () => consumerLoggerBuilder.BuildForApplicationTracing(
                        eventConsumerConfiguration.RollingLogPath,
                        eventConsumerConfiguration.Name,
                        eventConsumerConfiguration.ApplicationName)
                },
                {
                    Bus, () => consumerLoggerBuilder.BuildForBusTracing(
                        eventConsumerConfiguration.RollingLogPath,
                        eventConsumerConfiguration.Name,
                        eventConsumerConfiguration.ApplicationName)
                },
                {
                    SignalR, () => consumerLoggerBuilder.BuildForSignalRTracing(
                        eventConsumerConfiguration.RollingLogPath,
                        eventConsumerConfiguration.Name,
                        eventConsumerConfiguration.ApplicationName)
                }
            };
        }
    }
}