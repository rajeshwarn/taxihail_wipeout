#region

using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationDao
    {
        ServerPaymentSettings GetPaymentSettings();
    }
}