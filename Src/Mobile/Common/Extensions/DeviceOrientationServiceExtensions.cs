using System;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Enumeration;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class DeviceOrientationServiceExtensions
    {
        public static IObservable<DeviceOrientations> ObserveDeviceIsInLandscape(this IDeviceOrientationService service)
        {
			return Observable.FromEventPattern<EventHandler<DeviceOrientationChangedEventArgs>, DeviceOrientationChangedEventArgs>(
					handler => service.NotifyOrientationChanged += handler,
					handler => service.NotifyOrientationChanged -= handler
				)
				.Select(args => args.EventArgs.DeviceOrientation)
                .Where(deviceOrientation => deviceOrientation == DeviceOrientations.Left || deviceOrientation == DeviceOrientations.Right);
        }
    }
}
