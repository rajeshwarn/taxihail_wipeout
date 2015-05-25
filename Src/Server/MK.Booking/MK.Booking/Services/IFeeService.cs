using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface IFeeService
    {
        void ChargeNoShowFeeIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail);
    }
}