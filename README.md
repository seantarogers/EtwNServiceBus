# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider configured to be the default logger for NServiceBus

# EtwNServiceBus

## Overview

This is a lightweight, generic ETW Consumer. It consumes trace events from app.config defined Event Sources. The solution also contains an ETW Provider that has configured it's NServiceBus Bus instance to emit Debug and Error events to ETW.
The Consumer was written as a simplified alternative to [Semantic Logging Application Block](https://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx).

## To run solution

1. Add an NserviceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
2. Start the Provider, Consumer and Endpoint projects
3. Browse to http://localhost:8089/api/test
4. Observe the Debug and Error Trace events (from both application and NServicebus event sources) written to the Consumer Console window

## Performance Comparisons

The PerformanceComparisons console project contains 3 tests which compare Log4Net, EasyLogger and ETW over a 10 second period.  It measures how many traces they can each emit.

**TL;DR**

| Tracer            | Sync             | Number of traces in 10 seconds  |
| ----------------- | ---------------- | ------------------------------- |
| ETW               | to the O/S       | 8,495,000                       |
| EasyLogger        | to rolling file  | 3,484,000                       |
| Standard Log4Net  | to rolling file  |    68,900                       |                                  |

Clearly when there are more appenders used in Log4Net the performance degrades significantly. We are only using one appender in this test. The benefit of running this through ETW out of process is that the number of event syncs have zero impact on the Provider performance.

## Architecture

![Image of Architecture](https://github.com/seantarogers/EtwNServiceBus/blob/master/EtwNServiceBusOverview.png)
