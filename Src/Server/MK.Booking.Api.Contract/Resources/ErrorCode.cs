namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public enum ErrorCode
    {
        Ok,
// ReSharper disable InconsistentNaming
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
 // ReSharper restore InconsistentNaming
    }
}