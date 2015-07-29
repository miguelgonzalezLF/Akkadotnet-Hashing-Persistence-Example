using Akka.Actor;
using IEP.Shared.State;

namespace IEP.Shared.Commands
{
    public class StartJob : IStartJob
    {
        public StartJob(){}
        public StartJob(ChangeJob job)
        {
            Job = job;
        }
        public StartJob(ChangeJob job, long deliveryId)
        {
            DeliveryId = deliveryId;
            Job = job;
        }
        public ChangeJob Job { get; private set; }
        public long DeliveryId { get; private set; }

        public object ConsistentHashKey { get { return Job.FileName; } }
    }
}