using System;
using ObjCRuntime;

namespace Card.IO
{
    [Native]
    public enum CreditCardType : long
    {
        Unknown = 0,
        Unrecognized = 0,
        Ambiguous = 1,
        Amex = 51 /*'3'*/,
        JCB = 74 /*'J'*/,
        Visa = 52 /*'4'*/,
        Mastercard = 53 /*'5'*/,
        Discover = 54 /*'6'*/
    }

    [Native]
    public enum DetectionMode : long
    {
        CardImageAndNumber = 0,
        CardImageOnly,
        Automatic
    }
}

namespace PaypalSdkTouch.Unified
{
    [Native]
    public enum PayPalShippingAddressOption : long
    {
        None = 0,
        Provided = 1,
        PayPal = 2,
        Both = 3
    }

    [Native]
    public enum PayPalPaymentIntent : long
    {
        Sale = 0,
        Authorize = 1,
        Order = 2
    }

    [Native]
    public enum PayPalPaymentViewControllerState : long
    {
        Unsent = 0,
        InProgress = 1
    }
}

