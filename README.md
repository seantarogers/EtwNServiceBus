# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider configured to be the default logger for NServiceBus

## Overview

This is a lightweight, multi-threaded, generic ETW Consumer. It consumes trace events from configurable Event Sources. The solution also contains an ETW Provider that has configured it's NServiceBus Bus instance to emit Debug and Error trace events to ETW.
The Consumer was written as a simplified and easily deployable alternative to [the Semantic Logging Application Block (SLAB)](https://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx). The Provider was written as a high performance logger for NServiceBus.

## Performance Comparisons

The PerformanceComparisons project compares the tracing performance of in process log4net (3 appenders configured: RollingLogFile, ADO and Windows Event log) against out of process ETW over a 10 second period.  It measures how many traces they can each emit.  If you are just interested in the results:

| Tracer            | Sync                                        | Number of traces in 10 seconds |
| ----------------- | --------------------------------------------|--------------------------------|
| ETW               | to the O/S                                  |8,495,000                       |
| In proc Log4Net   | rolling file, Sql Db and Windows Event log  |      903                       |                                  |

## Buffering

This solution offers two levels of buffering of trace events. This is important for both performance and also to protect against event loss as the Consumer may not be able to keep up with the Provider.  Firstly, Etw will buffer trace events internally in it's buffer pool until the Consumer is available to read them.  Secondly, inside of the consumer we are agressively buffering both the writing to the database and the writing to the rolling log files.

## Buffer Flusher

The Buffer Flusher ensures that the second level buffers do cache stale data when there is low tracing activity.  The flusher will run every 100 seconds and flush each Event Consumer's buffers (the database buffers and the rolling log file buffers).  This allows us to provide robust buffering without compromise. 

## To Run Solution and view the Trace Results

1. Add an NserviceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
2. Start the Provider, Consumer and Endpoint projects
3. Browse to http://localhost:8089/api/test
4. Wait for 100 seconds: 
    + Observe the Error and InfoDebug database tables to view the debug and error traces from the Web Api 
    + Observe the C:\logs\Provider\application-all.log to view the debug and error traces from the Web Api 
    + Observe the C:\logs\Provider\bus-all.log to view the NServiceBus infrastructure logging
    + Observe the Windows Event Log\Applications and Services Logs\EtwConsumerLog to view error traces from the Web Api
    + Observe the C:\logs\Consumer\application-all.log to see the consumer logging
    
## Architecture

![Image of Architecture](https://github.com/seantarogers/EtwNServiceBus/blob/master/EtwNServiceBusArchitecture.png)
