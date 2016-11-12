﻿using Topshelf;

namespace Provider
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            const string serviceName = "NsbEtwProvider";

            HostFactory.Run(
                x =>
                {
                    x.Service<IServiceHost>(
                        s =>
                        {
                            s.ConstructUsing(pc => new ServiceHost());
                            s.WhenStarted((pc, hostControl) => pc.Start(hostControl));
                            s.WhenStopped(pc => pc.Stop());
                        });
                    x.RunAsLocalSystem();

                    x.SetDescription(serviceName);
                    x.SetDisplayName(serviceName);
                    x.SetServiceName(serviceName);

                    x.StartAutomaticallyDelayed();
                });
        }
    }
}
