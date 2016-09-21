namespace Consumer.Subscribers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Adapters;
    using Constants;

    using Consumer.Events;
    using Consumer.Functions;
    using Consumer.Queues;

    using CustomConfiguration;
    using Producers;

    using Easy.Logger;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;

    public class EventSubscriber : IEventSubscriber
    {
        private readonly ILogService logService;
        private readonly IDebugQueue debugQueue;
        private readonly IErrorQueue errorQueue;

        private readonly ITraceEventAdapter traceEventAdapter;

        private readonly ITraceSessionManager traceSessionManager;

        private ILogger applicationSourceEasyLogger;
        private ILogger consumerEasyLogger;
        private EventSubscriberConfigurationElement eventSubscriberConfiguration;
        private TraceEventSession traceEventSession;
        private Action<Exception> raiseErrorInParentThread;

        private const string Session = "session";

        public EventSubscriber(
            ILogService logService, 
            IDebugQueue debugQueue, 
            IErrorQueue errorQueue, 
            ITraceEventAdapter traceEventAdapter, 
            ITraceSessionManager traceSessionManager, 
            ILogger consumerEasyLogger)
        {
            this.logService = logService;
            this.debugQueue = debugQueue;
            this.errorQueue = errorQueue;
            this.traceEventAdapter = traceEventAdapter;
            this.traceSessionManager = traceSessionManager;
            this.consumerEasyLogger = consumerEasyLogger;
        }

        public void Start(EventSubscriberConfigurationElement eventSubscriberConfigurationElement)
        {
            eventSubscriberConfiguration = eventSubscriberConfigurationElement;
            applicationSourceEasyLogger = logService.GetLogger(eventSubscriberConfiguration.EasyLoggerName);
            consumerEasyLogger = logService.GetLogger("consumerLogger");
            
            try
            {
                traceEventSession = traceSessionManager.CreateTraceEventSession(
                    eventSubscriberConfiguration.EventSource + Session,
                    eventSubscriberConfiguration.EventSource);

                SubscribeToDebugEventStream();
                SubscribeToErrorEventStream();

                traceEventSession.Source.Process();
            }
            catch (Exception exception)
            {
                consumerEasyLogger.ErrorFormat( "An exception has been raised in Start(). For Source: {0} Details: {1}",
                    eventSubscriberConfiguration.EventSource, exception);
                raiseErrorInParentThread(exception);
            }
        }

        public void Stop()
        {
           traceSessionManager.DisposeTraceEventSession(
                eventSubscriberConfiguration.EventSource + Session, traceEventSession);
        }

        public void OnError(Action<Exception> errorAction)
        {
            raiseErrorInParentThread = errorAction;
        }
        
        private void SubscribeToErrorEventStream()
        {
            var errorEventStream = traceEventSession.Source.Dynamic.Observe(
                eventSubscriberConfiguration.EventSource,
                HostConstants.EventTypes.ErrorEvents);

            errorEventStream.Subscribe(
                traceEvent => AddErrorEventToQueue(),
                exception => consumerEasyLogger.Error(exception),
                () => consumerEasyLogger.DebugFormat(""));
        }

        private void SubscribeToDebugEventStream()
        {
            var debugEventStream = traceEventSession.Source.Dynamic.Observe(
                eventSubscriberConfiguration.EventSource,
                HostConstants.EventTypes.DebugEvents);

            debugEventStream.Subscribe(
                traceEvent => AddDebugEventToQueue(),
                exception => consumerEasyLogger.Error(exception),
                () => consumerEasyLogger.DebugFormat(""));
        }

        private void AddErrorEventToQueue()
        {
            errorQueue.Add(
                new ErrorTraceReceivedEvent
                    {
                        ClientAplicationName = eventSubscriberConfiguration.ApplicationName,
                        TraceEventAdapter = traceEventAdapter,
                        EasyLogger = applicationSourceEasyLogger
                    });
        }

        private void AddDebugEventToQueue()
        {
            debugQueue.Add(
                new DebugTraceReceivedEvent
                    {
                        ClientAplicationName = eventSubscriberConfiguration.ApplicationName,
                        TraceEventAdapter = traceEventAdapter,
                        EasyLogger = applicationSourceEasyLogger
                    });
        }
    }
}