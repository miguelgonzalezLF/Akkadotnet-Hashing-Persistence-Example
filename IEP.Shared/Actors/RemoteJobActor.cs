using System;
using Akka.Actor;
using Akka.Routing;
using IEP.Shared.Commands;

namespace IEP.Shared.Actors
{
    public class RemoteJobActor : ReceiveActor
    {
        public RemoteJobActor()
        {
            Receive<IStartJob>(startJob =>
            {
                //Console.WriteLine("RemoteJobActor HashCode: {0}", Self.GetHashCode());
                Context.ActorSelection("/user/api").Tell(startJob, Sender);
            });
        }

    }
}