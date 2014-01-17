using System;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public enum AuthFailure
    {
        NetworkError,
        InvalidServiceUrl,
        InvalidUsernameOrPassword,
        AccountNotActivated,
        AccountDisabled,
    }
}

