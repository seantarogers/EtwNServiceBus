using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Consumer.Consumers;
using Consumer.CustomConfiguration;
using Consumer.Extensions;
using Consumer.Functions;
using Consumer.Providers;
using log4net;
using log4net.Config;
using SimpleInjector;
using Topshelf;

namespace Consumer
{
    public class ServiceHost : IServiceHost
    {
        private List<IEventConsumer> eventConsumers;
        private IBufferFlusher bufferFlusher;
        private HostControl thisHostControl;
        private ILog logger;
        private Container container;
        private ConfigurationProvider configurationProvider;

        public bool Start(HostControl hostControl)
        {
            thisHostControl = hostControl;

            try
            {
                CreateContainer();
                CreateApplicationLogger();
                StartEventConsumers();
                StartBufferFlusher();
                logger.Debug("Consumer has started listening for traces...");
            }
            catch (AggregateException aggregateException)
            {
                LogAggregateException(aggregateException);
                return false;
            }
            catch (Exception exception)
            {
                logger?.ErrorFormat($"An exception was raised in the ServiceHost. Details: {exception}");
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            try
            {
                logger.ErrorFormat("Stop was called in the ServiceHost");
                StopEventConsumers();
                bufferFlusher.Flush();
            }
            catch (Exception exception)
            {
                logger.ErrorFormat($"An error was raised whilst trying to stop an event Consumer. Details: {exception}");
            }
            return true;
        }

        private void LogAggregateException(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                stringBuilder.Append(innerException);
            }

            logger.ErrorFormat($"An exception was raised in the ServiceHost. Details: {stringBuilder}");
        }

        private void StopEventConsumers()
        {
            foreach (var eventConsumer in eventConsumers)
            {
                logger.DebugFormat($"Stopping event consumer : {eventConsumer.Name}");
                eventConsumer.Stop();
                logger.DebugFormat($"Stopped event consumer: {eventConsumer.Name}");
            }
        }

        private void StartEventConsumers()
        {
            eventConsumers = new List<IEventConsumer>();
            foreach (EventConsumersElement eventConsumersElement in EventConsumersSection.Section.EventConsumers)
            {
                if (ConsumersShouldBeStarted(eventConsumersElement))
                {
                    continue;
                }

                foreach (EventConsumerConfigurationElement eventConsumerConfigurationElement in eventConsumersElement)
                {
                    var eventConsumer = container.GetInstance<IEventConsumer>();
                    logger.DebugFormat($"Starting event consumer: {eventConsumerConfigurationElement.Name}");
                    Task.Factory.StartNew(() => { eventConsumer.Start(eventConsumerConfigurationElement, ConsumerError); }, TaskCreationOptions.LongRunning);
                    eventConsumers.Add(eventConsumer);
                }
            }
        }

        private void ConsumerError(Exception exception)
        {
            logger.ErrorFormat($"An consumer error was raised. The windows service will be stopped. Details: {exception}");
            thisHostControl.Stop();
        }

        private bool ConsumersShouldBeStarted(EventConsumersElement eventConsumersElement) => eventConsumersElement.DeploymentLocation != configurationProvider.DeploymentLocation
                                                                                              && configurationProvider.DeploymentLocation != DeploymentLocationType.All;

        private void CreateContainer()
        {
            container = new Container();
            container.RegisterComponents();
            container.Verify();

            configurationProvider = container.GetInstance<ConfigurationProvider>();
        }

        private void CreateApplicationLogger()
        {
            CustomAdoNetAppenderInitializer.IntializeType();
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            logger = container.GetInstance<ILog>();
        }

        private void StartBufferFlusher()
        {
            bufferFlusher = container.GetInstance<IBufferFlusher>();
            bufferFlusher.Start(configurationProvider.BufferFlushIntervalInSeconds);
        }
    }
}