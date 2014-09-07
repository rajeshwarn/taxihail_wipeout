namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public enum ErrorCode
    {
// ReSharper disable InconsistentNaming
        CreateAccount_AccountAlreadyExist,
        CreateAccount_CannotSendSMS,
        CreateAccount_InvalidConfirmationToken,
        CreateOrder_CannotCreateInIbs,
        CreateOrder_SettingsRequired,
        CreateOrder_InvalidProvider,
        CreateOrder_RuleDisable,
        CreateOrder_NoFareEstimateAvailable,
        CreateOrder_PendingOrder,
        CreateOrder_CardOnFileButNoCreditCard,
        NearbyPlaces_LocationRequired,
        Search_Locations_NameRequired,
        UpdatePassword_NotSame,
        OrderNotInIbs,
        OrderNotCompleted,
        Tariff_DuplicateName,
        Rule_DuplicateName,
        Rule_InvalidPriority,
        ResetPassword_AccountNotFound,
        AccountCharge_AccountAlreadyExisting,
 // ReSharper restore InconsistentNaming
    }
}