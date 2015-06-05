using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface IFeeService
    {
        bool ChargeNoShowFeeIfNecessary(OrderStatusDetail orderStatusDetail);

        bool ChargeCancellationFeeIfNecessary(OrderStatusDetail orderStatusDetail);
    }
}