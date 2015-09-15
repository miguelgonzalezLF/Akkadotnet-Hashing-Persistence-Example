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
                    Deliver(DeliveryPath, id =>
                    {
                        Print(
                            string.Format("recovered delivery task: {0}, with deliveryId: {1}", msg.Job.FileName, id),
                            msg.Job.Color);
                        return new StartJob(msg.Job, id);
                    });
                })
                .With<JobConfirmation>(msg =>
                {
                    Print(string.Format("recovered confirmation of {0}", msg.DeliveryId), msg.Color);
                    if(ConfirmDelivery(msg.DeliveryId))
                        DeleteMessages(msg.DeliveryId, true); 
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
                .With<JobConfirmation>(msg => 
                {
                    var deliveryId = msg.DeliveryId;
                    Print(string.Format("Command confirmation of {0}", deliveryId), msg.Color);
                    if(ConfirmDelivery(deliveryId))   
                        DeleteMessages(deliveryId, true); 
                })
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