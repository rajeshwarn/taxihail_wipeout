using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface ICreditCardAuthorizationService
    {
        string Authorize(CreditCardInfos creditCardRequest);
    }
}

