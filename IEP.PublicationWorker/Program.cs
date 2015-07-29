using Topshelf;

namespace IEP.PublicationWorker
{
    class Program
    {
        static int Main(string[] args)
        {
            return (int)HostFactory.Run(x =>
            {
                x.SetServiceName("Publication");
                x.SetDisplayName("Publication");
                x.SetDescription("Publication cluster");

                x.UseAssemblyInfoForServiceInfo();
                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.Service<HostService>();
                x.EnableServiceRecovery(r => r.RestartService(1));
            });
        }
    }
}
