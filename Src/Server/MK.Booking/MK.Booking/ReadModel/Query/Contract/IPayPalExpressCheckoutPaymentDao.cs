﻿namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IPayPalExpressCheckoutPaymentDao
    {
        PayPalExpressCheckoutPaymentDetail FindByToken(string token);
    }
}
