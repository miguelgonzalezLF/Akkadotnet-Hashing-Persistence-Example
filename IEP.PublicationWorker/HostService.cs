using Akka.Actor;
using Akka.Routing;
using IEP.PublicationWorker.Actors;
using Topshelf;

namespace IEP.PublicationWorker
{
    internal class HostService : ServiceControl
    {
        protected ActorSystem ClusterSystem { get; set; }

        public bool Start(HostControl hostControl)
        {
            ClusterSystem = ActorSystem.Create("sys");
            ClusterSystem.ActorOf(Props.Create(() => new ApiMaster()).WithRouter(FromConfig.Instance), "api");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            ClusterSystem.Shutdown();
            return true;
        }
    }
}