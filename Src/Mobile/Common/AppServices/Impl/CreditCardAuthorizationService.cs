using System;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class CreditCardAuthorizationService : ICreditCardAuthorizationService
    {
        public CreditCardAuthorizationService ()
        {
        }

        #region ICreditCardAuthorizationService implementation

        public string Authorize (CreditCardInfos creditCardRequest)
        {
            return "aDfRTG85963ddde";
        }

        #endregion
    }
}

