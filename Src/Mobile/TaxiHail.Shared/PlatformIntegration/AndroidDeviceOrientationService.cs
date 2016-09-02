using System;
using apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation;
using apcurium.MK.Common.Diagnostic;
using Android.App;
using Android.Content;
using Android.Hardware;
using Cirrious.MvvmCross.Platform;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AndroidDeviceOrientationService : CommonDeviceOrientationService
	{
		private readonly SensorManager _sensorManager;
		private readonly Sensor _accelerometer;
		private readonly AccelerometerSensorListener _accelerometerSensorListener;
		private bool _enabled;

		public AndroidDeviceOrientationService(IMvxLifetime mvxLifetime, ILogger logger) : base(Common.CoordinateSystemOrientation.LeftHanded, mvxLifetime, logger)
		{
			_sensorManager = (SensorManager)Application.Context.GetSystemService(Context.SensorService);
			_accelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);

			if (_accelerometer != null)
			{
				_accelerometerSensorListener = new AccelerometerSensorListener();
				_accelerometerSensorListener.NotifyOrientationChanged += OrientationChanged;
			}
		}

		public override bool IsAvailable()
		{
			return _accelerometer != null;
		}

		protected override bool StartService()
		{
			if (IsAvailable() && !_enabled)
			{
				_enabled = _sensorManager.RegisterListener(_accelerometerSensorListener, _accelerometer, SensorDelay.Ui);
				return _enabled;
			}

			return false;
		}

		protected override bool StopService()
		{
			if (IsAvailable() && _enabled)
			{
				_sensorManager.UnregisterListener(_accelerometerSensorListener);
				_enabled = false;
				return true;
			}

			return false;
		}
	}

	class AccelerometerSensorListener : Java.Lang.Object, ISensorEventListener
	{
		public event Action<double, double, double, long> NotifyOrientationChanged;

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{
		}

		public void OnSensorChanged(SensorEvent e)
		{
			if (e.Sensor.Type == SensorType.Accelerometer
                && (e.Accuracy == SensorStatus.AccuracyHigh
                    || e.Accuracy == SensorStatus.AccuracyMedium
                    || e.Accuracy == SensorStatus.AccuracyLow))
			{
				if (NotifyOrientationChanged != null)
				{
                    // double x, double y, double z, long timestamp
                    NotifyOrientationChanged(e.Values[0], e.Values[1], e.Values[2], e.Timestamp / 1000000);
				}
			}
		}
	}
}