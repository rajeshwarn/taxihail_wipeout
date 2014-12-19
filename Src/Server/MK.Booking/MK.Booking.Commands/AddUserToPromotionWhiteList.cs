using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddUserToPromotionWhiteList : ICommand
    {
        public AddUserToPromotionWhiteList()
        {
            Id = Guid.NewGuid();
        }
        
        public Guid AccountId { get; set; }

        public Guid PromoId { get; set; }

        public Guid Id { get; private set; }
    }
}
