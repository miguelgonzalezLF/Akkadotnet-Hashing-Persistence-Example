using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Topshelf;

namespace IEP.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            ObjectFactory.Initialize(cfg =>
            {
                cfg.For<IHost>().Singleton().Use<HostService>();
            });

            HostFactory.Run(config =>
            {
                config.RunAsLocalSystem();
                config.SetDisplayName("Test.AppHost");
                config.SetServiceName("Test.AppHost");
                config.SetDescription("Test.AppHost");

                config.Service<IHost>(service =>
                {
                    service.ConstructUsing(builder => ObjectFactory.GetInstance<IHost>());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
            });
        }
    }
}
