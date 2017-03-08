namespace PerformanceComparisons
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using log4net;
    using log4net.Config;

    using EventSources;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Comparing Log4Net against ETW over 10 seconds...");
            
            CustomAdoNetAppenderInitializer.IntializeType();
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));

            //1. Test push traces to log4net to rolling file appender for 10 seconds
            //RunStandardLog4NetTest();
            
            //3. Test push traces to ETW for 10 seconds
            RunEtwTest();
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
                applicationEventSource.Error("Program", $"ETW error statement. Number of traces: {traceNumber}");
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("ETW: {0:n0} traces written to OS", traceNumber);
            Console.WriteLine("");
            Console.WriteLine("GC Gen 0: {0}", GC.CollectionCount(0));
            Console.WriteLine("GC Gen 1: {0}", GC.CollectionCount(1));
            Console.WriteLine("GC Gen 2: {0}", GC.CollectionCount(2));
            Console.WriteLine("========================");
        }

        private static void RunStandardLog4NetTest()
        {
            var log4NetLogger = LogManager.GetLogger(typeof(Program));

            var duration = TimeSpan.FromSeconds(10);

            var sw = Stopwatch.StartNew();
            long traceNumber = 0;
            while (sw.Elapsed < duration)
            {
                traceNumber++;
                log4NetLogger.ErrorFormat("log4net error statement. Number of traces: {0}", traceNumber);
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("Standard in proc Log4Net: {0:n0} traces written to log file" , traceNumber);
            Console.WriteLine("");
            Console.WriteLine("GC Gen 0: {0}", GC.CollectionCount(0));
            Console.WriteLine("GC Gen 1: {0}", GC.CollectionCount(1));
            Console.WriteLine("GC Gen 2: {0}", GC.CollectionCount(2));
            Console.WriteLine("========================");
            Console.WriteLine("Now running Etw over 10 seconds...");
        }
    }
}
