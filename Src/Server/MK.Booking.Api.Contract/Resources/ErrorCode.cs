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
        CreateOrder_NoProvider,
        CreateOrder_InvalidProvider,
        CreateOrder_VehiculeType,
        CreateOrder_RuleDisable,
        CreateOrder_NoFareEstimateAvailable,
        NearbyPlaces_LocationRequired,
        Search_Locations_NameRequired,
        UpdatePassword_NotSame,
        OrderNotInIbs,
        OrderNotCompleted,
        Tariff_DuplicateName,
        Rule_DuplicateName,
        Rule_InvalidPriority,
        ResetPassword_AccountNotFound
    }
}
