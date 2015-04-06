using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using CMTPayment.Pair;

namespace apcurium.MK.Booking.Services
{
    public interface IManualRideLinqService
    {
        Trip PairRideLinqTrip(OrderStatusDetail orderStatusDetail);
        Trip GetTripInfo(Guid orderId);
    }
}
