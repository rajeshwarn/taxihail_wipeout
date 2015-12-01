#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class PayGratuity: ICommand
    {
        public PayGratuity()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public int Percentage { get; set; }
        
        public Guid Id { get; set; }
    }
}