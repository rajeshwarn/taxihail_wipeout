using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class RemoveUserFromPromotionWhiteList : ICommand
    {
        public RemoveUserFromPromotionWhiteList()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }

        public Guid PromoId { get; set; }

        public Guid Id { get; private set; }
    }
}
