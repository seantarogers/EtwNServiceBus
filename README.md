# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider configured to be the default logger for NServiceBus

## Overview

This is a lightweight, multi-threaded, generic ETW Consumer. It consumes trace events from configurable Event Sources. The solution also contains an ETW Provider that has configured it's NServiceBus Bus instance to emit debug and error trace events to ETW.
  The Consumer was written as a simplified and easily deployable alternative to the Semantic Logging Application Block. The Provider was written as a high performance logger for NServiceBus.

## Performance Comparisons

The PerformanceComparisons project compares the tracing performance of in-process Log4Net (with 3 appenders configured: RollingLogFile, ADONet and Windows Event Log) against out-of-process ETW, over a 10 second period.  It measures how many traces they can each emit.  If you are just interested in the results:

| Tracer            | Sync                                        | Number of traces in 10 seconds |
| ----------------- | --------------------------------------------|--------------------------------|
| ETW               | to the O/S                                  |8,495,000                       |
| In proc Log4Net   | Rolling File, Sql Db and Windows Event Log  |      903                       |                                  

This test merely compares the rate at which the Provider could emit traces.  It does not attempt to compare the consumption of traces. 
However, as the Consumer is a non blocking, out of process service, the rate of consumption becomes much less important.  
  Nevertheless, in the interests of experimentation, with the first level ETW buffering (see next section) turned up to 300MB, and second level buffering set to 10,000 the Consumer was able to successfully consume and persist 3 million debug trace events in 4 minutes with zero events lost.

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

No, the Consumer service is simply an ETW consumer. It can used to just listen for standard application trace events.

## To Run Solution And view the Trace Results

1. Add an NServiceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
2. Start the Provider, Consumer and Endpoint projects
3. Browse to http://localhost:8089/api/test
4. Wait for 100 seconds (or change the BufferFlusher interval configuration) and check the following:
    + Error and InfoDebug database tables to view the debug and error traces from the Web Api 
    + C:\logs\Provider\application-all.log to view the debug and error traces from the Web Api 
    + C:\logs\Provider\bus-all.log to view the NServiceBus infrastructure logging
    + Windows Event Log\Applications and Services Logs\EtwConsumerLog to view error traces from the Web Api
    + C:\logs\Consumer\application-all.log to see the Consumer service logging
    
## Deployment

The ETW Consumer should be deployed next to the Providers that are emiting the trace events.  So typically you would have one Consumer per logical server and that would provide event consumption for all sites and services running on that server.
  To support this, the Consumer service allows deployment of a sub set of consumers to specific named servers. It avoids the overhead of having to deploy and run all consumers on all servers.  For each server deployment set the DeploymentLocation:  
```<appSettings>
    <add key="DeploymentLocation" value="WebServer" /> 
```
and then specify the set of EventConsumers you want to run on that DeploymentLocation like so:
```
 <eventConsumersSection>
    <eventConsumers deploymentLocation="WebServer"> 
      <eventConsumer name="Consumer1" eventSource="Provider1-Application-EventSource" applicationName="Provider1" eventType="Application" rollingLogPath="c:\Logs\Provider1\" />     
      <eventConsumer name="Consumer2" eventSource="Provider2-Bus-EventSource" applicationName="Provider2" eventType="Bus" rollingLogPath="c:\Logs\Provider2\" />     
     </eventConsumers>     
  </eventConsumersSection>
  <appSettings>
```
## Application v Bus Event Types

The Consumer was designed with service bus infrastructure events in mind. For this reason, it distinguishes between Application trace events and Bus trace events. Bus traces can be quite noisy so only Bus errors should be written to the database. However, all Application traces (Debug and Error) should be written to the database for analysis.  This switch is achieved via the following eventType configuration setting:
```
 <eventConsumersSection>
    <eventConsumers deploymentLocation="WebServer"> 
      <eventConsumer eventType="Application or Bus here" />
      </eventConsumers>
  </eventConsumersSection>
  <appSettings>

```
