using System;
using Akka.Actor;
using IEP.Shared.Commands;

namespace IEP.PublicationWorker.Actors
{
    public class ApiMaster : ReceiveActor
    {
        public ApiMaster()
        {
            Receive<IStartJob>(message =>
            {
                Console.BackgroundColor = message.Job.Color;
                Console.WriteLine("FileName: {0}, change: {1} hashcode: {2}", message.Job.FileName, message.Job.Change, Self.GetHashCode());
                Console.ResetColor();
                Sender.Tell(new JobConfirmation(message.DeliveryId, message.Job.Color));
            });
        }
    }
}