using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Consumer.Consumers;
using Consumer.CustomConfiguration;
using Consumer.Extensions;
using SimpleInjector;
using Topshelf;

namespace Consumer
{
    public class ServiceHost : IServiceHost
    {
        private List<IEventConsumer> eventProducers;
        private static HostControl serviceHostControl;
        
        public static Container Container { get; private set; }

        public bool Start(HostControl hostControl)
        {
            serviceHostControl = hostControl;
            
            try
            {
                CreateContainer();
                StartEventConsumers();
            }
            catch (AggregateException aggregateException)
            {
                LogAggregateException(aggregateException);
                return false;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An exception was raised in the ServiceHost. Details: {exception}");
                return false;
            }

            return true;
        }        

        public bool Stop()
        {
            Console.WriteLine("Stop was called in the ServiceHost");
            return StopEventConsumers();
        }
        
        private static void LogAggregateException(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                stringBuilder.Append(innerException);
            }

            Console.WriteLine($"An exception was raised in the ServiceHost. Details: {stringBuilder}");
        }

        private static void ConsumerError(Exception exception)
        {
            Console.WriteLine($"An consumer error was raised. The windows service will be stopped. Details: {exception}");
            serviceHostControl.Stop();
        }

        private bool StopEventConsumers()
        {
            try
            {
                foreach (var eventProducer in eventProducers)
                {
                    var producerName = eventProducer.GetType().FullName;
                    Console.WriteLine($"Stopping event consumer : { producerName}");
                    eventProducer.Stop();
                    Console.WriteLine($"Stopped event consumer : { producerName}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An error was raised whilst trying to stop an eventConsumer.Details: { exception}");
                return false;
            }
            return true;
        }

        private void StartEventConsumers()
        {
            eventProducers = new List<IEventConsumer>();
            var eventProducerElements = EventConsumersSection.Section.EventConsumerElements;
            for (var i = 0; i < eventProducerElements.Count; i++)
            {
                var eventSubscriberConfiguration = eventProducerElements[i];

                var eventSubscriber = Container.GetInstance<IEventConsumer>();
                Console.WriteLine($"Starting event Consumer: {eventSubscriber.GetType().FullName}");

                Task.Factory.StartNew(() => { eventSubscriber.Start(eventSubscriberConfiguration, ConsumerError); },
                    TaskCreationOptions.LongRunning);

                eventProducers.Add(eventSubscriber);
            }
        }

        private static void CreateContainer()
        {
            Container = new Container();
            Container.UseExecutionContextLifestyle();
            Container.RegisterComponents();    
            Container.Verify();        
        }
    }
}