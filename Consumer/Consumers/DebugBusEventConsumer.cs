using System;
using ASI.Log4Net.Contracts;
using Easy.Logger;
using PFTracing.Application.Adapters;
using PFTracing.Application.Command.Payloads;
using PFTracing.Application.Command.Services;
using PFTracing.Application.Events;
using PFTracing.Etw.Host.Adapters;
using PFTracing.Etw.Host.Constants;

namespace PFTracing.Etw.Host.Consumers
{
    public class DebugBusEventConsumer : IEventConsumer
    {
        private readonly IDebugBusQueueAdapter debugBusQueue;
        private readonly IAsiLogger pfTracingAsiLogger;
        private readonly IContainerAdapter containerAdapter;

        private Action<Exception> raiseErrorInParentThread;

        public DebugBusEventConsumer(
            IDebugBusQueueAdapter debugBusQueue,
            IAsiLogger pfTracingAsiLogger,
            IContainerAdapter containerAdapter)
        {
            this.debugBusQueue = debugBusQueue;
            this.pfTracingAsiLogger = pfTracingAsiLogger;
            this.containerAdapter = containerAdapter;
        }

        public void Start()
        {
            try
            {
                foreach (var debugBusEventReceived in debugBusQueue.GetConsumingEnumerable())
                {
                    SinkEvent(debugBusEventReceived);
                }
            }
            catch (Exception exception)
            {
                pfTracingAsiLogger.ErrorFormat(this, HostConstants.DebugBusEventConsumerError, exception);
                raiseErrorInParentThread(exception);
            }
        }

        private void SinkEvent(TraceReceivedEvent debugBusEventReceived)
        {
            using (containerAdapter.BeginLifetimeScope())
            {
                var eventPayload = CreateEventPayload(debugBusEventReceived);
                if (!eventPayload.IsValid)
                {
                    pfTracingAsiLogger.DebugFormat(this, HostConstants.DebugBusEventWithoutPayload,
                        debugBusEventReceived.ClientAplicationName);
                    return;
                }

                LogDebugEventToRollingFile(eventPayload, debugBusEventReceived.EasyLogger);
            }
        }

        private EventPayload CreateEventPayload(TraceReceivedEvent debugBusEventReceived)
        {
            var eventPayloadBuilder = containerAdapter.GetInstance<IEventPayloadBuilder>();
            return eventPayloadBuilder.Build(debugBusEventReceived.TraceEventAdapter);            
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