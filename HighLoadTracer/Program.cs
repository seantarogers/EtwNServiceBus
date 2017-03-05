using System;
using HighLoadTracer.EventSources;

namespace HighLoadTracer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("About to dump 1M traces into ETW...");
            var applicationEventSource = new ApplicationEventSource();
            var i = 0;
            while (i <= 1000000)
            {
                applicationEventSource.Debug("Program", $"ETW debug statement. Trace number: {i}");
                i++;
            }
            
            Console.WriteLine("Dumped 1M traces into ETW");
            Console.ReadLine();
        }
    }
}
