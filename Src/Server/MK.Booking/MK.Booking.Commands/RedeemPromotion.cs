using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class RedeemPromotion : ICommand
    {
        public RedeemPromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }
        public Guid OrderId { get; set; }
        public decimal TotalAmountOfOrder { get; set; }

        public Guid Id { get; private set; }
    }
}