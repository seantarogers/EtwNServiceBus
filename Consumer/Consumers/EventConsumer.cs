using System;
using Consumer.Adapters;
using Consumer.Constants;
using Consumer.CustomConfiguration;
using Consumer.Functions;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace Consumer.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly IEventPayloadBuilder eventPayloadBuilder;
        private readonly ITraceSessionManager traceSessionManager;
        private EventConsumerConfigurationElement eventConsumerConfiguration;
        private TraceEventSession traceEventSession;
        
        private const string Session = "-Session";

        public EventConsumer(
            ITraceSessionManager traceSessionManager,
            IEventPayloadBuilder eventPayloadBuilder)
        {
            this.traceSessionManager = traceSessionManager;
            this.eventPayloadBuilder = eventPayloadBuilder;
        }

        public void Start(EventConsumerConfigurationElement eventConsumerConfigurationElement, Action<Exception> raiseException)
        {
            eventConsumerConfiguration = eventConsumerConfigurationElement;
            
            try
            {
                CreateTraceEventSession();
                SubscribeToDebugTraceEventStream();
                SubscribeToErrorTraceEventStream();
                StartTraceEventSession();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception raised in Event Consumer: {exception}");
                raiseException(exception);
            }
        }

        public void Stop()
        {
            traceSessionManager.DisposeTraceEventSession(
                eventConsumerConfiguration.EventSource + Session,
                traceEventSession);
        }
        
        private void StartTraceEventSession()
        {
            traceEventSession.Source.Process();
        }

        private void CreateTraceEventSession()
        {
            traceEventSession =
                traceSessionManager.CreateTraceEventSession(
                    eventConsumerConfiguration.EventSource + Session,
                    eventConsumerConfiguration.EventSource);
        }

        private void SubscribeToDebugTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(
                eventConsumerConfiguration.EventSource,
                ConsumerConstants.DebugLevel);

            eventStream.Subscribe(
                traceEvent => DebugEventSink(new TraceEventAdapter(traceEvent)),
                exception =>
                Console.WriteLine(
                    $"An exception was raised whilst consuming an debug event from the event stream. Event processing will now stop. Event Source: {eventConsumerConfiguration.EventSource}, Details: {exception}"),
                () =>
                Console.WriteLine(
                    $"The debug event stream has completed for source: {eventConsumerConfiguration.EventSource}."));
        }

        private void SubscribeToErrorTraceEventStream()
        {
            var eventStream = traceEventSession.Source.Dynamic.Observe(
                eventConsumerConfiguration.EventSource,
                ConsumerConstants.ErrorLevel);

            eventStream.Subscribe(
                traceEvent => ErrorEventSink(new TraceEventAdapter(traceEvent)),
                exception =>
                Console.WriteLine(
                    $"An exception was raised whilst consuming an error event from the event stream. Event processing will now stop. Event Source: {eventConsumerConfiguration.EventSource}, Details: {exception}"),
                () =>
                Console.WriteLine(
                    $"The error event stream has completed for source: {eventConsumerConfiguration.EventSource}."));
        }
        
        private void ErrorEventSink(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                Console.WriteLine("event payload is not valid");
                return;
            }

            Console.WriteLine("==========================================");
            Console.WriteLine($"ERROR event received. Event source: {eventConsumerConfiguration.EventSource}, application: {eventConsumerConfiguration.ApplicationName} {eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss")} {eventPayload.TraceSource} {eventPayload.Payload}");
            Console.WriteLine("==========================================");
            Console.WriteLine("");
        }
        
        private void DebugEventSink(ITraceEventAdapter traceEventAdapter)
        {
            var eventPayload = eventPayloadBuilder.Build(traceEventAdapter);
            if (!eventPayload.IsValid)
            {
                Console.WriteLine("event payload is not valid");
                return;
            }

            Console.WriteLine("==========================================");
            Console.WriteLine($"DEBUG event received. Event source: {eventConsumerConfiguration.EventSource}, application: {eventConsumerConfiguration.ApplicationName} {eventPayload.TraceDate.ToString("yyyy-MM-dd HH:mm:ss")} {eventPayload.TraceSource} {eventPayload.Payload}");
            Console.WriteLine("==========================================");
            Console.WriteLine("");
        }
    }
}