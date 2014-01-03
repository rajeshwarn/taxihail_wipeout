using System;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPaymentService :IPaymentServiceClient
    {        
        double? GetPaymentFromCache(Guid orderId);
        
        void SetPaymentFromCache(Guid orderId, double amount);        
    }
}

