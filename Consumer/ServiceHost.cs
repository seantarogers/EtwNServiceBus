namespace Consumer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Extensions;
    
    using Producers;

    using CustomConfiguration;

    using SimpleInjector;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        private List<IEventConsumer> eventProducers;
        private static HostControl thisHostControl;

        //private ILogger easyLogger;

        public static Container Container { get; private set; }

        public bool Start(HostControl hostControl)
        {
            thisHostControl = hostControl;
            SetUpLog4Net();
            
            try
            {
                CreateContainer();
                StartEventProducers();
            }
            catch (AggregateException aggregateException)
            {
                LogAggregateException(aggregateException);
                return false;
            }
            catch (Exception exception)
            {
                //easyLogger.ErrorFormat($"An exception was raised in the ServiceHost. Details: {exception}");
                return false;
            }

            return true;
        }        

        public bool Stop()
        {
            //easyLogger.ErrorFormat("Stop was called in the ServiceHost");
            
            return StopEventProducers();
        }

        private static void LogAggregateException(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                stringBuilder.Append(innerException);
            }

            //easyLogger.ErrorFormat($"An exception was raised in the ServiceHost. Details: {stringBuilder}");
        }
        

        private bool StopEventProducers()
        {
            try
            {
                foreach (var eventProducer in eventProducers)
                {
                    var producerName = eventProducer.GetType().FullName;
                    //easyLogger.DebugFormat($"Stopping event producer : {producerName}");
                    eventProducer.Stop();
                    //easyLogger.DebugFormat($"Stopped event producer: {producerName}");
                }
            }
            catch (Exception exception)
            {
                //easyLogger.ErrorFormat($"An error was raised whilst trying to stop an eventProducer. Details: {exception}");
                return false;
            }
            return true;
        }

        private void StartEventProducers()
        {
            eventProducers = new List<IEventConsumer>();
            var eventProducerElements = EventConsumersSection.Section.EventConsumerElements;
            for (var i = 0; i < eventProducerElements.Count; i++)
            {
                var eventSubscriberConfiguration = eventProducerElements[i];

                var eventSubscriber = Container.GetInstance<IEventConsumer>();
                eventSubscriber.OnError(SubscriberError);
                //easyLogger.DebugFormat($"Starting event producer: {eventSubscriber.GetType().FullName}");

                Task.Factory.StartNew(() => { eventSubscriber.Start(eventSubscriberConfiguration); },
                    TaskCreationOptions.LongRunning);

                eventProducers.Add(eventSubscriber);
            }
        }
        
        private void SubscriberError(Exception exception)
        {
            //easyLogger.ErrorFormat("An subscriber error was raised. " +
                                        //$"The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }
        

        private static void CreateContainer()
        {
            Container = new Container();
            Container.UseExecutionContextLifestyle();
            Container.RegisterComponents();    
            Container.Verify();        
        }

        private void SetUpLog4Net()
        {
            //EasyLogger.InitializeTypeReference();
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            //easyLogger = Log4NetService.Instance.GetLogger(GetType());
        }
    }
}