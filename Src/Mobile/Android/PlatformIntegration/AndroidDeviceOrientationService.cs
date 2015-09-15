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
using Android.Hardware;

namespace apcurium.MK.Booking.Mobile.Client.Services
{
	public class AndroidDeviceOrientationService : CommonDeviceOrientationService, IDeviceOrientationService
	{
		class AccelerometerSensorListener : Java.Lang.Object, ISensorEventListener
		{
			public event Action<double, double, double, long> NotifyOrientationChanged;

			public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
			{
			}

			public void OnSensorChanged(SensorEvent e)
			{
				if (e.Accuracy == SensorStatus.AccuracyHigh || e.Accuracy == SensorStatus.AccuracyMedium || e.Accuracy == SensorStatus.AccuracyLow)
				{
					if (e.Sensor.Type == SensorType.Accelerometer)
					{
						if (NotifyOrientationChanged != null)
						{
							NotifyOrientationChanged(e.Values[0], e.Values[1], e.Values[2], (long)(e.Timestamp / 1000000));
						}
					}
				}
			}
		}

		SensorManager sensorManager;
		Sensor accelerometer;
		AccelerometerSensorListener accelerometerSensorListener;
		bool enabled = false;

		public AndroidDeviceOrientationService():base(CoordinateSystemOrientation.LeftHanded)
		{
			sensorManager = (SensorManager)Application.Context.GetSystemService(Context.SensorService);
			accelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

			if (accelerometer != null)
			{
				accelerometerSensorListener = new AccelerometerSensorListener();
				accelerometerSensorListener.NotifyOrientationChanged += OrientationChanged;
			}
		}

		public override bool IsAvailable()
		{
			return accelerometer != null;
		}

		protected override bool StartService()
		{
			if (IsAvailable() && !enabled)
			{
				sensorManager.RegisterListener(accelerometerSensorListener, accelerometer, SensorDelay.Normal);
				enabled = true;
				return true;
			}

			return false;
		}

		protected override bool StopService()
		{
			if (IsAvailable() && enabled)
			{
				sensorManager.UnregisterListener(accelerometerSensorListener);
				enabled = true;
				return true;
			}

			return false;
		}
	}

/*


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
 */
}