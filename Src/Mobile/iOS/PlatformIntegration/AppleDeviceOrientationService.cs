using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using UIKit;
using Foundation;
using CoreMotion;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class AppleDeviceOrientationService: CommonDeviceOrientationService, IDeviceOrientationService
    {
		private const double AccelerometerUpdateInterval = 1 / 5; // 5 Hz

        private CMMotionManager _motionManager;
		private NSOperationQueue _accelerometerUpdateQueue;

        public AppleDeviceOrientationService()
        {
			if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.DEVICE)
			{
				_motionManager = new CMMotionManager();
			}
        }

        public override bool IsAvailable()
        {
			if (_motionManager != null)
			{
				return _motionManager.DeviceMotionAvailable && _motionManager.AccelerometerAvailable;
			}
			else
			{
				return false;
			}
        }

        protected override bool StartService()
        {
            if (_motionManager != null && IsAvailable())
            {
                _accelerometerUpdateQueue = new NSOperationQueue();
                _motionManager.AccelerometerUpdateInterval = AccelerometerUpdateInterval;
                _motionManager.StartAccelerometerUpdates(_accelerometerUpdateQueue, AngleChangedEvent);
                return true;
            }

            return false;
        }

        protected override bool StopService()
        {
            if (_motionManager != null && IsAvailable())
            {
                _motionManager.StopAccelerometerUpdates();
                return true;
            }

            return true;
        }

        void AngleChangedEvent(CMAccelerometerData data, NSError error)
        {
            if (error == null)
            {
                OrientationChanged(data.Acceleration.X, data.Acceleration.Y, data.Acceleration.Z, (long)(data.Timestamp * 1000));
            }
        }
    }
}