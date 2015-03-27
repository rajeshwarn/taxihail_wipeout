using System;

namespace apcurium.MK.Booking.Events
{
    public class UserAddedToPromotionWhiteList_V2 : UserAddedToPromotionWhiteList
    {
        public Guid[] AccountIds { get; set; }
    }
}
