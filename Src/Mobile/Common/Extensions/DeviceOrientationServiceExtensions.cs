using System;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class DeviceOrientationServiceExtensions
    {
        public static IObservable<DeviceOrientations> ObserveDeviceIsInLandscape(this IDeviceOrientationService service)
        {
			return Observable.FromEventPattern<EventHandler<DeviceOrientationChangedEventArgs>, DeviceOrientationChangedEventArgs>(
					h => service.NotifyOrientationChanged += h,
					h => service.NotifyOrientationChanged -= h
				)
				.Select(args => args.EventArgs.DeviceOrientation)
                .Where(deviceOrientation => deviceOrientation == DeviceOrientations.Left || deviceOrientation == DeviceOrientations.Right);
        }
    }
}
