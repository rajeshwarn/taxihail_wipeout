using System;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public interface IExtensionPoint
    {
        object ExtendedValue { get; }
        Type ExtendedType { get; }
    }
}