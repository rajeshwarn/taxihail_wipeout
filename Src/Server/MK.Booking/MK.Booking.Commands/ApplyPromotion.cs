using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ApplyPromotion : ICommand
    {
        public ApplyPromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime PickupDate { get; set; }
        public bool IsFutureBooking { get; set; } 

        public Guid Id { get; private set; }
    }
}