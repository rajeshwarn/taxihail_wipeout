using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
    public class RemovePromotion : ICommand
    {
        public RemovePromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }

        public Guid Id { get; private set; }
    }
}