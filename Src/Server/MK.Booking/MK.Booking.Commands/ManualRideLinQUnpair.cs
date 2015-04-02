using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ManualRideLinQUnpair : ICommand
    {
        public ManualRideLinQUnpair()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string RideLinqId { get; set; }
    }
}
