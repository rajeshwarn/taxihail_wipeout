
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;


namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPaymentService :IPaymentServiceClient
    {        
        bool GetIsOrderPayed(Guid orderId);
        
        void SetIsOrderPayed(Guid orderId);

    }
}

