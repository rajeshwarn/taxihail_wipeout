using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using UIKit;
using Foundation;
using CoreMotion;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class AppleDeviceOrientationService: CommonDeviceOrientationService, IDeviceOrientationService
    {
		private const double AccelerometerUpdateInterval = 1 / 20; // 20 Hz

        private CMMotionManager _motionManager;
		private NSOperationQueue _accelerometerUpdateQueue;

		// we don't use exclusive access here because the consequences are negligible - may cause one additional OrientationChanged event after stop service
		private bool _isStarted = false;

		private Thread orientationUpdateThread;

		public AppleDeviceOrientationService():base(Common.CoordinateSystemOrientation.RightHanded)
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
                if (orientationUpdateThread != null && orientationUpdateThread.IsAlive)
                {
                    return false;
                }

				_motionManager.StartAccelerometerUpdates();

				_isStarted = true;
				orientationUpdateThread = new Thread(OrientationUpdateThread);
				orientationUpdateThread.Priority = ThreadPriority.BelowNormal;
				orientationUpdateThread.Start();

                return true;
            }

            return false;
        }

        protected override bool StopService()
        {
            _isStarted = false;

            if (_motionManager != null && IsAvailable())
            {
                _motionManager.StopAccelerometerUpdates();
				return true;
            }

            return false;
        }

		void OrientationUpdateThread()
		{
			while (_isStarted)
			{
                if (_motionManager.AccelerometerData != null)
                {
                    OrientationChanged(_motionManager.AccelerometerData.Acceleration.X, _motionManager.AccelerometerData.Acceleration.Y, _motionManager.AccelerometerData.Acceleration.Z, (long)(_motionManager.AccelerometerData.Timestamp * 1000));
                }

                if (!_isStarted)
                {
                    break;
                }

				Thread.Sleep((int)(1000 * AccelerometerUpdateInterval));
			}
		}
    }
}