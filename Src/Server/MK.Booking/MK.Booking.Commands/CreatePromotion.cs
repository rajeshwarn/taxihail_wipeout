using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CreatePromotion : ICommand
    {
        public CreatePromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }
        public Guid Id { get; set; }
    }
}