namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public interface IExtensionPoint<T> : IExtensionPoint
    {
        new T ExtendedValue { get; }
    }
}