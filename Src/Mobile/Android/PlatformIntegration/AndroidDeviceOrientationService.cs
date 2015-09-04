using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Services
{
	public class AndroidDeviceOrientationService : CommonDeviceOrientationService, IDeviceOrientationService
	{
		AndroidOrientationListener androidOrientationListener;

		public AndroidDeviceOrientationService()
		{
			androidOrientationListener = new AndroidOrientationListener(Application.Context, Android.Hardware.SensorDelay.Normal);
		}

		public override bool IsAvailable()
		{
			return androidOrientationListener.CanDetectOrientation();
		}

		protected override bool StartService()
		{
            if (IsAvailable())
			{
				androidOrientationListener.NotifyOrientationChanged += AngleChangedEvent;
				androidOrientationListener.Enable();
				return true;
			}

			return false;
		}

		protected override bool StopService()
		{
            if (IsAvailable())
			{
				androidOrientationListener.Disable();
				androidOrientationListener.NotifyOrientationChanged -= AngleChangedEvent;
				return true;
			}

			return false;
		}
	}

	public class AndroidOrientationListener : OrientationEventListener
	{
		public event Action<int> NotifyOrientationChanged;

		public AndroidOrientationListener(Context context)
			: base(context)
		{
		}

		public AndroidOrientationListener(Context context, Android.Hardware.SensorDelay rate)
			: base(context, rate)
		{
		}

		public override void OnOrientationChanged(int angle)
		{
			if (NotifyOrientationChanged != null)
			{
				NotifyOrientationChanged(angle);
			}
		}
	}
}