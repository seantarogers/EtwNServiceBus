using System;

namespace Provider
{
    using Microsoft.Owin.Hosting;

    internal static class Program
    {
        static void Main(string[] args)
        {
            using (var webHost = WebApp.Start("http://localhost:8093"))
            {
                Console.WriteLine("Successfully started the api host on: {0}", "http://localhost:8093");
                Console.ReadLine();
            }
        }
    }
}
