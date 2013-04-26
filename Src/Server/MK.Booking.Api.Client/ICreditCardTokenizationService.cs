using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace apcurium.MK.Booking.Api.Client
{
    public interface ICreditCardTokenizationService
    {
        TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate);
        TokenizeDeleteResponse ForgetTokenizedCard(string cardToken);
    }
}

