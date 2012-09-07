using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public enum ErrorCode
    {
        Ok,
        CreateAccount_AccountAlreadyExist,
        CreateOrder_InvalidPickupAddress,
        CreateOrder_CannotCreateInIbs,
        CreateOrder_SettingsRequired,
        CancelOrder_OrderNotInIbs,
        NearbyPlaces_LocationRequired,
        Search_Locations_NameRequired
    }
}
