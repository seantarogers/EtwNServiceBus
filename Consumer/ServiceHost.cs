namespace Consumer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Consumers;
    using Events;
    using Producers;
    using Queues;

    using CustomConfiguration;

    using Easy.Logger;

    using Exensions;

    using log4net.Config;
    
    using SimpleInjector;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        private List<IEventProducer> eventProducers;
        private IEnumerable<IEventConsumer> eventConsumers;
        private static IEventQueue<DebugTraceReceivedEvent> debugQueue;
        private static IEventQueue<ErrorTraceReceivedEvent> errorQueue;
        private static HostControl thisHostControl;

        private ILogger easyLogger;

        public static Container Container { get; private set; }

        public bool Start(HostControl hostControl)
        {
            thisHostControl = hostControl;
            SetUpLog4Net();
            
            try
            {
                CreateContainer();
                StartEventConsumers();
                StartEventProducers();
                StoreQueuesForlaterDisposal();
            }
            catch (AggregateException aggregateException)
            {
                LogAggregateException(aggregateException);
                return false;
            }
            catch (Exception exception)
            {
                easyLogger.ErrorFormat($"An exception was raised in the ServiceHost. Details: {exception}");
                return false;
            }

            return true;
        }        

        public bool Stop()
        {
            easyLogger.ErrorFormat("Stop was called in the ServiceHost");

            if (!StopConsumersConsumingFromQueues())
            {
                return false;
            }

            return StopEventProducers();
        }

        private void LogAggregateException(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                stringBuilder.Append(innerException);
            }

            easyLogger.ErrorFormat($"An exception was raised in the ServiceHost. Details: {stringBuilder}");
        }

        private bool StopConsumersConsumingFromQueues()
        {
            try
            {
                easyLogger.DebugFormat("Stopping errorQueue");
                errorQueue?.CompleteAdding();
                easyLogger.DebugFormat("Stopped errorQueue");

                easyLogger.DebugFormat("Stopping debugQueue");
                debugQueue?.CompleteAdding();
                easyLogger.DebugFormat("Stopped debugQueue");                
            }
            catch (Exception exception)
            {
                easyLogger.ErrorFormat($"An error was raised whilst trying to stop a queue. Details: {exception}");
                return false;
            }
            return true;
        }

        private bool StopEventProducers()
        {
            try
            {
                foreach (var eventProducer in eventProducers)
                {
                    var producerName = eventProducer.GetType().FullName;
                    easyLogger.DebugFormat($"Stopping event producer : {producerName}");
                    eventProducer.Stop();
                    easyLogger.DebugFormat($"Stopped event producer: {producerName}");
                }
            }
            catch (Exception exception)
            {
                easyLogger.ErrorFormat($"An error was raised whilst trying to stop an eventProducer. Details: {exception}");
                return false;
            }
            return true;
        }

        private static void StoreQueuesForlaterDisposal()
        {
            debugQueue = Container.GetInstance<IEventQueue<DebugTraceReceivedEvent>>();
            errorQueue = Container.GetInstance<IEventQueue<ErrorTraceReceivedEvent>>();
        }

        private void StartEventProducers()
        {
            eventProducers = new List<IEventProducer>();
            var eventProducerElements = EventProducersSection.Section.EventProducerElements;
            for (var i = 0; i < eventProducerElements.Count; i++)
            {
                var eventSubscriberConfiguration = eventProducerElements[i];

                var eventSubscriber = Container.GetInstance<IEventProducer>();
                eventSubscriber.OnError(SubscriberError);
                easyLogger.DebugFormat($"Starting event producer: {eventSubscriber.GetType().FullName}");

                Task.Factory.StartNew(() => { eventSubscriber.Start(eventSubscriberConfiguration); },
                    TaskCreationOptions.LongRunning);

                eventProducers.Add(eventSubscriber);
            }
        }

        private void StartEventConsumers()
        {
            eventConsumers = Container.GetAllInstances<IEventConsumer>();
            foreach (var eventConsumer in eventConsumers)
            {
                eventConsumer.OnError(ConsumerError);
                easyLogger.DebugFormat($"Starting event consumer: {eventConsumer.GetType().FullName}");
                Task.Factory.StartNew(() => { eventConsumer.Start(); }, TaskCreationOptions.LongRunning);
            }
        }

        private void SubscriberError(Exception exception)
        {
            easyLogger.ErrorFormat("An subscriber error was raised. " +
                                        $"The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }

        private void ConsumerError(Exception exception)
        {
            easyLogger.ErrorFormat("A consumer error was raised. " +
                                        $"The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }

        private static void CreateContainer()
        {
            Container = new Container();
            Container.UseLifetimeScopeLifestyle();
            Container.RegisterComponents();            
        }

        private void SetUpLog4Net()
        {
            EasyLogger.InitializeTypeReference();
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            easyLogger = Log4NetService.Instance.GetLogger(GetType());
        }
    }
}