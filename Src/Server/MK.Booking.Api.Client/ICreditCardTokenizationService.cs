using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface ICreditCardTokenizationService
    {
        TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate);
    }
}

