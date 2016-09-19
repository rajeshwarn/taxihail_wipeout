#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateOrderGratuityTimer : ICommand
    {
        public UpdateOrderGratuityTimer()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public DateTime WaitingForExtraGratuityStartDate { get; set; }

        public Guid Id { get; set; }
    }
}