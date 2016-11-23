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
        private List<IEventConsumer> eventConsumers;
        private static HostControl serviceHostControl;

        private static Container Container { get; set; }

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
                foreach (var eventConsumer in eventConsumers)
                {
                    var producerName = eventConsumer.GetType().FullName;
                    Console.WriteLine($"Stopping event consumer : { producerName}");
                    eventConsumer.Stop();
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
            eventConsumers = new List<IEventConsumer>();
            var eventConsumerElements = EventConsumersSection.Section.EventConsumerElements;
            for (var i = 0; i < eventConsumerElements.Count; i++)
            {
                var eventConsumerConfigurationElement = eventConsumerElements[i];

                var eventConsumer = Container.GetInstance<IEventConsumer>();
                Console.WriteLine($"Starting event Consumer: {eventConsumer.GetType().FullName}");

                Task.Factory.StartNew(() => { eventConsumer.Start(eventConsumerConfigurationElement, ConsumerError); },
                    TaskCreationOptions.LongRunning);

                eventConsumers.Add(eventConsumer);
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