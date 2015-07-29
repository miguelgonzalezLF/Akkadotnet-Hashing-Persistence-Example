using Akka.Actor;
using Akka.Routing;
using IEP.Shared.State;

namespace IEP.Shared.Commands
{
    public interface IStartJob : IConsistentHashable
    {
        ChangeJob Job { get; }
        long DeliveryId { get; }
    }
}