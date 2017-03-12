# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider which is configured to be the default logger for NServiceBus

## Consumer

The Consumer project is a lightweight, multi-threaded, generic ETW Consumer and ETW Controller.  It is responsible for creating event stream sessions (Controller) and then subscribing to them using the Reactive Extensions library (Consumer). It can be configured to listen to create and subscribe to multiple event streams concurrently. It was written as a simple, modern and easy to deploy alternative to the Semantic Logging Application Block.

## Provider

The Consumer project is OWIN hosted Web API that also hosts a Send Only NServiceBus instance. The Provider emits application trace events and it emits NServiceBus infrastructure trace events. The traces are sent to two different event stream sessions "Bus" and "Application". Both streams have been created and subscribed to by the Consumer. The Provider uses the EventSource type to emit traces. Likewise, NServiceBus has been configured to use ETW rather than it's default Log4Net logger.

## Performance Comparisons

The PerformanceComparisons project compares the tracing performance of in-process Log4Net (with 3 appenders configured: RollingLogFile, ADONet and Windows Event Log) against out-of-process ETW, over a 10 second period.  It measures how many traces they can each emit.  If you are just interested in the results:

| Tracer            | Sync                                        | Number of traces in 10 seconds |
| ----------------- | --------------------------------------------|--------------------------------|
| ETW Provider      | ETW Session                                 |8,495,000                       |
| In proc Log4Net   | Rolling File, Sql Db and Windows Event Log  |      903                       |                                  

This test compares the rate at which the Provider could emit traces.  It does not attempt to compare the consumption and delivery of traces. However, it illustrates the almost zero latency tracing power of ETW. Moreover, as the Consumer is out of process and completely non blocking, the rate of consumption becomes much less important.

## First Level Buffering

This solution offers two levels of buffering of trace events. This is important for both performance and also to protect against event loss as the Consumer may not be able to keep up with the Provider.  
  First level buffering is provided by ETW. Each ETW session will buffer trace events internally in it's buffer pool until the Consumer is available to read them. The default is 64MB per session, but this can be increased via the app.config if needed.

## Second Level Buffering

Second level buffering is provided by the Consumer service. It agressively buffers both the writing of events to the database and the writing of events to the rolling log files. Currently, this buffer is set to 1000 trace events. The Windows Event Log trace writing is not buffered. This is because the existance of these events are used by SCOM to trigger real time email alerts to interested parties.

## Buffer Flusher

The Buffer Flusher ensures that the second level buffers do not go stale when there is low tracing activity. The flusher will run every 100 seconds and flush each Event Consumer's buffers (the database buffers and the rolling log file buffers). This allows us to provide robust buffering without compromise. The flusher is also executed when the ServiceHost.Stop() method is triggered on service shutdown.

## Architecture

![Image of Architecture](https://github.com/seantarogers/EtwNServiceBus/blob/master/ETWNServiceBusArchitecture.png)

## Do I need to be using NServiceBus to use this?

No, the Consumer service is simply an ETW Consumer. It can used to create trace sessions for and subscribe to any type of trace events. 

## To Run Solution And view the Trace Results

1. Run the CreateTraceTablesAndSprocs.sql to create the SQL Sink tables and Sprocs.
2. Add an NServiceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
3. Run the Provider, Consumer and Endpoint projects
4. Browse to http://localhost:8089/api/test
5. Wait for 100 seconds (or change the BufferFlusher interval configuration) and check the following:
    + Error and InfoDebug database tables to view the debug and error traces from the Web Api 
    + C:\logs\Provider\application-all.log to view the debug and error traces from the Web Api 
    + C:\logs\Provider\bus-all.log to view the NServiceBus infrastructure logging
    + Windows Event Log\Applications and Services Logs\EtwConsumerLog to view error traces from the Web Api
    + C:\logs\Consumer\application-all.log to see the Consumer service logging
    
## Deployment Strategy

The ETW Consumer should be deployed next to the Providers that are emiting the trace events.  So typically you would have one Consumer per logical server and that would provide event consumption for all sites and services running on that server.

```
