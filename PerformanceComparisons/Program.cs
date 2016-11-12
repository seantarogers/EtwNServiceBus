namespace PerformanceComparisons
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using Easy.Logger;

    using log4net;
    using log4net.Config;

    using PerformanceComparisons.EventSources;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Comparing Log4Net and EasyLogger against ETW over 10 seconds...");

            EasyLogger.InitializeTypeReference();
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));

            //1. Test push traces to log4net to rolling file appender for 10 seconds
            Task.Run(() => RunStandardLog4NetTest());

            //2. Test push traces to easy logger to rolling file appender for 10 seconds
            Task.Run(() => RunEasyLoggerFastLog4NetLoggerTest());

            //3. Test push traces to ETW for 10 seconds
            Task.Run(() => RunEtwTest());

            Console.WriteLine("");
            Console.WriteLine("(Once the results are reported, press any key to see number of garbage collections...)");
            Console.ReadLine();
            
            Console.WriteLine("GC Gen 0: {0}", GC.CollectionCount(0));
            Console.WriteLine("GC Gen 1: {0}", GC.CollectionCount(1));
            Console.WriteLine("GC Gen 2: {0}", GC.CollectionCount(2));

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
            Console.WriteLine("ETW: {0:n0} traces written to OS", traceNumber);
            Console.WriteLine("========================");
            Console.WriteLine("");
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
                log4NetLogger.DebugFormat("log4net debug statement. Number of traces: {0}", traceNumber);
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("Standard Log4Net: {0:n0} traces written to log file" , traceNumber);
            Console.WriteLine("========================");
            Console.WriteLine("");
        }

        private static void RunEasyLoggerFastLog4NetLoggerTest()
        {
            var easyLogger = Log4NetService.Instance.GetLogger("EasyLogger");
            var duration = TimeSpan.FromSeconds(10);

            var sw = Stopwatch.StartNew();
            long traceNumber = 0;
            while (sw.Elapsed < duration)
            {
                traceNumber++;
                easyLogger.DebugFormat("easyLogger debug statement. Number of traces: {0}", traceNumber);
            }

            Console.WriteLine("");
            Console.WriteLine("========================");
            Console.WriteLine("Easy Logger: {0:n0} traces written to log file", traceNumber);
            Console.WriteLine("========================");
            Console.WriteLine("");
        }
    }
}
