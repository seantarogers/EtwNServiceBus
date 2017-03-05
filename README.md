# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider configured to be the default logger for NServiceBus

## Overview

This is a lightweight, multi-threaded, generic ETW Consumer. It consumes trace events from configurable Event Sources. The solution also contains an ETW Provider that has configured it's NServiceBus Bus instance to emit Debug and Error events to ETW.
The Consumer was written as a simplified and easily deployable alternative to [the Semantic Logging Application Block (SLAB)](https://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx). The Provider was written as a high performance logger for NServiceBus.

## Performance Comparisons

The PerformanceComparisons console project contains 3 tests which compare Log4Net, EasyLogger and ETW over a 10 second period.  It measures how many traces they can each emit.  If you are just interested in the results:

| Tracer            | Sync             | Number of traces in 10 seconds  |
| ----------------- | ---------------- | ------------------------------- |
| ETW               | to the O/S       | 8,495,000                       |
| EasyLogger        | to rolling file  | 3,484,000                       |
| Standard Log4Net  | to rolling file  |    68,900                       |                                  |

When more Log4Net appenders are used the overall performance degrades significantly. We are only using one appender in this test. However when you add in an ADO appender and a Windows EventLog Appender the number of traces per 10 seconds heads down towards 1000. Fortunately, when writing to an out of process ETW Consumer there is a negligible overhead on the Provider performance. As a result, the Provider can trace as frequently as required and distribute these traces to as many sources as required.

## To Run Solution

1. Add an NserviceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
2. Start the Provider, Consumer and Endpoint projects
3. Browse to http://localhost:8089/api/test
4. Observe the Debug and Error Trace events (from both application and NServicebus event sources) written to the Consumer Console window

## Architecture

![Image of Architecture](https://github.com/seantarogers/EtwNServiceBus/blob/master/EtwNServiceBusOverview.png)
