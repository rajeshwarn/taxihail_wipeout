using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationDao
    {
        ServerPaymentSettings GetPaymentSettings();
    }
}