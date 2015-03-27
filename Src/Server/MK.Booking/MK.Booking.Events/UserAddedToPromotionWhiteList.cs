using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    [Obsolete("Replaced by UserAddedToPromotionWhiteList", false)]
    public class UserAddedToPromotionWhiteList : VersionedEvent
    {
        [Obsolete("This field is obsolete. Use AccountIds from UserAddedToPromotionWhiteList_V2 instead")]
        public Guid AccountId { get; set; }

        public double? LastTriggeredAmount { get; set; }
    }
}
