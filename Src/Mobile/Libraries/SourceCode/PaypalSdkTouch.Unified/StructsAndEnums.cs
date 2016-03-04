using System;
using ObjCRuntime;

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

