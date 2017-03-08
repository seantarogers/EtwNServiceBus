namespace HighLoadProvider
{
    using System;

    using EventSources;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("About to dump 3M traces into ETW...");

            var applicationEventSource = new ApplicationEventSource();
            var i = 0;
            while (i <= 3000000)
            {
                applicationEventSource.Debug("Program", $"ETW debug statement. Trace number: {i}");
                i++;
            }

            Console.WriteLine("Dumped 3M traces into ETW");

            Console.ReadLine();
        }
    }
}
