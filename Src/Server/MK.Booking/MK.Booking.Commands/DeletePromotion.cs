using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
    public class DeletePromotion : ICommand
    {
        public DeletePromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }

        public Guid Id { get; private set; }
    }
}