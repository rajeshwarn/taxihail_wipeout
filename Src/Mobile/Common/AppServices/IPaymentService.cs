using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPaymentService :IPaymentServiceClient
    {        
        double? GetPaymentFromCache(Guid orderId);
        
        void SetPaymentFromCache(Guid orderId, double amount);     

		Task<ClientPaymentSettings> GetPaymentSettings(bool cleanCache = false);

		void ClearPaymentSettingsFromCache();
    }
}