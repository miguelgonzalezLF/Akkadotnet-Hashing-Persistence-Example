using System;
using System.Configuration;
using Akka.Actor;
using Akka.Persistence.Sqlite;
using Akka.Routing;
using IEP.Host.Actors;
using IEP.Shared.Actors;
using IEP.Shared.Commands;
using IEP.Shared.State;
using StructureMap;

namespace IEP.Host
{
    internal class HostService : IHost
    {
        private RESTServiceHost _restServiceHost;
        private static readonly string REST_SERVICE_URL = ConfigurationManager.AppSettings.Get("REST_ServiceURL");
        protected ActorSystem ClusterSystem;

        public void Start()
        {
            ClusterSystem = ActorSystem.Create("sys");
            
            SqlitePersistence.Get(ClusterSystem);

            var router = ClusterSystem.ActorOf(Props.Create(() => new RemoteJobActor()).WithRouter(FromConfig.Instance), "tasker");
            
            var commandExecutor = ClusterSystem.ActorOf(Props.Create(() => new CommandExecutor(router)), "commands");

            ObjectFactory.Initialize(cfg => cfg.For<IActorRef>().Singleton().Use(commandExecutor).Named("commands"));

            _restServiceHost = new RESTServiceHost();
            _restServiceHost.Init();
            _restServiceHost.Start(REST_SERVICE_URL);
        }

        public void Stop()
        {
            ClusterSystem.Shutdown();
            _restServiceHost.Stop();
            _restServiceHost.Dispose();
        }
    }
}