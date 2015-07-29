using System;

namespace IEP.Shared.Commands
{
    public class JobConfirmation
    {
        public JobConfirmation(long deliveryId, ConsoleColor color)
        {
            this.DeliveryId = deliveryId;
            Color = color;
        }

        public ConsoleColor Color { get; set; }
        public long DeliveryId { get; private set; }
    }
}