# EtwNServiceBus

Topshelf hosted ETW Consumer consuming trace events from an ETW Provider configured to be the default logger for NServiceBus

# ETWNServiceBus

## Overview

This is a lightweight, generic ETW Consumer. It consumes trace events from app.config defined Event Sources. The solution also contains an ETW Provider that has configured it's NServiceBus Bus instance to emit Debug and Error events to ETW.
The Consumer was written as a simplified alternative to [Semantic Logging Application Block](https://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx).

## To run solution

1. Add an NserviceBus license to C:\NServiceBus\License.xml (skip this step if just interested in the consumer)
2. Start the Provider, Consumer and Endpoint projects
3. Browse to http://localhost:8089/api/test
4. Observe the Debug and Error Trace events (from both application and NServicebus event sources) written to the Consumer Console window

## Architecture

![Image of Architecture](https://raw.githubusercontent.com/seantarogers/ETWNServiceBus/master/ETWNServiceBus.png)
