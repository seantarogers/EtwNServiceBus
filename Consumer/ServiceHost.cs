namespace Consumer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Consumer.CustomConfiguration;
    using Consumer.Exensions;
    using Consumer.Queues;
    using Consumer.Subscribers;

    using SimpleInjector;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        private static IEnumerable<IEventConsumer> eventConsumers;
        private static List<IEventSubscriber> eventSubscribers;
        private static IDebugQueue debugQueue;
        private static IErrorQueue errorQueue;
        private static IAsiLogger asiLogger;
        private static HostControl thisHostControl;

        public static Container Container { get; private set; }

        public bool Start(HostControl hostControl)
        {
            thisHostControl = hostControl;
            SetUpLog4Net();
            asiLogger = new AsiLogger();
            eventSubscribers = new List<IEventSubscriber>();

            try
            {
                CreateContainer();
                StartEventConsumers();
                StartEventSubscribers();
                StoreQueuesForlaterDisposal();
            }
            catch (AggregateException aggregateException)
            {
                LogAggregateException(aggregateException);
                return false;
            }
            catch (Exception exception)
            {
                asiLogger.ErrorFormat(this, $"An exception was raised in the ServiceHost. Details: {exception}");
                return false;
            }

            return true;
        }        

        public bool Stop()
        {
            asiLogger.ErrorFormat(this, "Stop was called in the ServiceHost");

            if (!StopConsumersConsumingFromQueues())
            {
                return false;
            }

            return StopEventSubscribers();
        }

        private void LogAggregateException(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                stringBuilder.Append(innerException);
            }

            asiLogger.ErrorFormat(this, $"An exception was raised in the ServiceHost. Details: {stringBuilder}");
        }

        private bool StopConsumersConsumingFromQueues()
        {
            try
            {
                asiLogger.DebugFormat(this, "Stopping errorQueue");
                errorQueue?.CompleteAdding();
                asiLogger.DebugFormat(this, "Stopped errorQueue");

                asiLogger.DebugFormat(this, "Stopping debugQueue");
                debugQueue?.CompleteAdding();
                asiLogger.DebugFormat(this, "Stopped debugQueue");

                asiLogger?.DebugFormat(this, "Stopping debugBusQueue");
                debugBusQueue.CompleteAdding();
                asiLogger.DebugFormat(this, "Stopped debugBusQueue");
            }
            catch (Exception exception)
            {
                asiLogger.ErrorFormat(this,
                    $"An error was raised whilst trying to stop a queue. Details: {exception}");
                return false;
            }
            return true;
        }

        private bool StopEventSubscribers()
        {
            try
            {
                foreach (var eventSubscriber in eventSubscribers)
                {
                    var eventSubscriberName = eventSubscriber.GetType().FullName;
                    asiLogger.DebugFormat(this, $"Stopping event subscriber: {eventSubscriberName}");
                    eventSubscriber.Stop();
                    asiLogger.DebugFormat(this, $"Stopped event subscriber: {eventSubscriberName}");
                }
            }
            catch (Exception exception)
            {
                asiLogger.ErrorFormat(this,
                    $"An error was raised whilst trying to stop an eventSubscriber. Details: {exception}");
                return false;
            }
            return true;
        }

        private static void StoreQueuesForlaterDisposal()
        {
            debugQueue = Container.GetInstance<IDebugQueueAdapter>();
            errorQueue = Container.GetInstance<IErrorQueueAdapter>();
            debugBusQueue = Container.GetInstance<IDebugBusQueueAdapter>();
        }

        private void StartEventSubscribers()
        {
            var eventSubscriberElements = EventSubscribersSection.Section.EventSubscriberElements;
            for (var i = 0; i < eventSubscriberElements.Count; i++)
            {
                var eventSubscriberConfiguration = eventSubscriberElements[i];

                var eventSubscriber = Container.GetInstance<IEventSubscriber>();
                eventSubscriber.OnError(SubscriberError);
                asiLogger.DebugFormat(this, $"Starting event subscriber: {eventSubscriber.GetType().FullName}");

                Task.Factory.StartNew(() => { eventSubscriber.Start(eventSubscriberConfiguration); },
                    TaskCreationOptions.LongRunning);

                eventSubscribers.Add(eventSubscriber);
            }
        }

        private void StartEventConsumers()
        {
            eventConsumers = Container.GetAllInstances<IEventConsumer>();
            foreach (var eventConsumer in eventConsumers)
            {
                eventConsumer.OnError(ConsumerError);
                asiLogger.DebugFormat(this, $"Starting event consumer: {eventConsumer.GetType().FullName}");
                Task.Factory.StartNew(() => { eventConsumer.Start(); }, TaskCreationOptions.LongRunning);
            }
        }

        private void SubscriberError(Exception exception)
        {
            asiLogger.ErrorFormat(this, "An subscriber error was raised. " +
                                        $"The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }

        private void ConsumerError(Exception exception)
        {
            asiLogger.ErrorFormat(this, "A consumer error was raised. " +
                                        $"The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }

        private static void CreateContainer()
        {
            Container = new Container();
            Container.UseLifetimeScopeLifestyle();
            Container.RegisterComponents();            
        }

        private static void SetUpLog4Net()
        {
            EasyLogger.InitializeTypeReference();
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
        }
    }
}