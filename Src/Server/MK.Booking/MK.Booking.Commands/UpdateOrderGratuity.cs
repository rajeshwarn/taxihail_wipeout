#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateOrderGratuity: ICommand
    {
        public UpdateOrderGratuity()
        {
            Id = Guid.NewGuid();
        }

		public Guid AccountId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }
        
        public Guid Id { get; set; }
    }
}