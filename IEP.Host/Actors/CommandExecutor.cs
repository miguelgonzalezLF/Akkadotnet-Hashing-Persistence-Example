using System;
using Akka;
using Akka.Actor;
using Akka.Persistence;
using IEP.Shared.Commands;

namespace IEP.Host.Actors
{
    public class CommandExecutor : AtLeastOnceDeliveryActor
    {
        public ActorPath DeliveryPath { get; private set; }

        public CommandExecutor(IActorRef CommandRouter)
        {
            DeliveryPath = CommandRouter.Path;
        }

        protected override bool ReceiveRecover(object message)
        {
            return message.Match()
                .With<IStartJob>(msg =>
                {
                    Print(string.Format("recovered delivery task: {0}, with deliveryId: {1}", msg.Job.FileName, msg.DeliveryId), msg.Job.Color);
                    Deliver(DeliveryPath, id => new StartJob(msg.Job, id));
                })
                .With<JobConfirmation>(msg =>
                {
                    Print(string.Format("recovered confirmation of {0}", msg.DeliveryId), msg.Color);
                    ConfirmDelivery(msg.DeliveryId);
                })
                .WasHandled;
        }

        protected override bool ReceiveCommand(object message)
        {
            return message.Match()
                .With<IStartJob>(msg => Persist(msg, m => Deliver(DeliveryPath, id =>
                {
                    Print(string.Format("FileName: {0}, change: {1} , with deliveryId: {2}", m.Job.FileName, m.Job.Change, id), msg.Job.Color);
                    return new StartJob(m.Job, id);
                })))
                .With<JobConfirmation>(msg => Persist(msg, m =>
                {
                    var deliveryId = msg.DeliveryId;
                    Print(string.Format("recovered confirmation of {0}", deliveryId), msg.Color);
                    ConfirmDelivery(deliveryId);                        
                }))
                .WasHandled;

        }

        public override string PersistenceId
        {
            get { return "CommandExecutor-1"; }
        }

        private void Print(string message, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();            
        }
    }
}