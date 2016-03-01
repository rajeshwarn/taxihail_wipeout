using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateOrderInTrip : ICommand
    {
        public UpdateOrderInTrip()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public Address DropOffAddress { get; set; }
    }
}
