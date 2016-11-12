using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint
{
    using Topshelf;

    class Program
    {
        static void Main(string[] args)
        {
            const string serviceName = "NsbEndpoint";

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
