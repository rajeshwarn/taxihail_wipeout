using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserAddedToPromotionWhiteList_V2 : UserAddedToPromotionWhiteList
    {
        public Guid[] AccountIds { get; set; }
    }
}
