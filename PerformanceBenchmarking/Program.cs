using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using PerformanceBenchmarking.EventSources;

namespace PerformanceBenchmarking
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Comparing Log4Net against ETW over 10 seconds...");

            //1. Test push traces to log4net to rolling file appender for 10 seconds
            Task.Run(() => RunLog4NetTest());

            //1. Test push traces to ETW for 10 seconds
            Task.Run(() => RunEtwTest());

            Console.ReadLine();

        }

        private static void RunEtwTest()
        {
            var duration = TimeSpan.FromSeconds(10);

            var applicationEventSource = new ApplicationEventSource();

            var sw = Stopwatch.StartNew();
            long traceNumber = 0;
            while (sw.Elapsed < duration)
            {
                traceNumber++;
                applicationEventSource.Debug("Program", $"ETW debug statement. Number of traces: {traceNumber}");
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("Number of ETW traces written to OS: {0:n0}, Time Taken: {1}", traceNumber, sw.Elapsed);
            Console.WriteLine("ETW GC Gen 0: {0}", GC.CollectionCount(0));
            Console.WriteLine("ETW GC Gen 1: {0}", GC.CollectionCount(1));
            Console.WriteLine("ETW GC Gen 2: {0}", GC.CollectionCount(2));
            Console.WriteLine("========================");
            Console.WriteLine("");
        }

        private static void RunLog4NetTest()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            var log4NetLogger = LogManager.GetLogger(typeof(Program));

            var duration = TimeSpan.FromSeconds(10);

            var sw = Stopwatch.StartNew();
            long traceNumber = 0;
            while (sw.Elapsed < duration)
            {
                traceNumber++;
                log4NetLogger.DebugFormat("log4net debug statement. Number of traces: {0}", traceNumber);
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("Number of Log4net traces written to logfile: {0:n0}, Time Taken: {1}", traceNumber, sw.Elapsed);
            Console.WriteLine("Log4net GC Gen 0: {0}", GC.CollectionCount(0));
            Console.WriteLine("Log4net GC Gen 1: {0}", GC.CollectionCount(1));
            Console.WriteLine("Log4net GC Gen 2: {0}", GC.CollectionCount(2));
            Console.WriteLine("========================");
            Console.WriteLine("");
        }
    }
}
