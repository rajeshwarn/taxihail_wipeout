using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface IFeeService
    {
        decimal? ChargeBookingFeesIfNecessary(OrderStatusDetail orderStatusDetail);

        bool CouldBeChargedNoShowFee(OrderStatusDetail orderStatusDetail);
        decimal? ChargeNoShowFeeIfNecessary(OrderStatusDetail orderStatusDetail);

        decimal? ChargeCancellationFeeIfNecessary(OrderStatusDetail orderStatusDetail);
    }
}