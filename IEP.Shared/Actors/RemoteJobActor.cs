using System;
using Akka.Actor;
using IEP.Shared.Commands;

namespace IEP.Shared.Actors
{
    public class RemoteJobActor : ReceiveActor
    {
        public RemoteJobActor()
        {
            Receive<IStartJob>(message =>
            {
//                Context.ActorSelection("/user/api").Tell(startJob, Sender);
                Console.BackgroundColor = message.Job.Color;
                Console.WriteLine("FileName: {0}, change: {1} hashcode: {2}", message.Job.FileName, message.Job.Change, Self.GetHashCode());
                Console.ResetColor();
                Sender.Tell(new JobConfirmation(message.DeliveryId, message.Job.Color));
            });
        }

    }
}