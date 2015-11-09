using apcurium.MK.Booking.Mobile.AppServices;
using Foundation;
using CoreMotion;
using System.Threading;
using apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class AppleDeviceOrientationService: CommonDeviceOrientationService
    {
		private readonly CMMotionManager _motionManager;

		// we don't use exclusive access here because the consequences are negligible - may cause one additional OrientationChanged event after stop service
		private bool _isOrientationUpdateThreadActive;
		private Thread _orientationUpdateThread;

		public AppleDeviceOrientationService() : base(Common.CoordinateSystemOrientation.RightHanded)
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
            return false;
        }

        protected override bool StartService()
        {
            if (_motionManager != null && IsAvailable())
            {
                if (_orientationUpdateThread != null && _orientationUpdateThread.IsAlive)
                {
                    return false;
                }

				_motionManager.StartAccelerometerUpdates();

				_isOrientationUpdateThreadActive = true;

                _orientationUpdateThread = new Thread(OrientationUpdateThread);
				_orientationUpdateThread.Priority = ThreadPriority.BelowNormal;
				_orientationUpdateThread.Start();

                return true;
            }

            return false;
        }

        protected override bool StopService()
        {
            _isOrientationUpdateThreadActive = false;

            if (_motionManager != null && IsAvailable())
            {
                _motionManager.StopAccelerometerUpdates();
				return true;
            }

            return false;
        }

        private void OrientationUpdateThread()
		{
			while (_isOrientationUpdateThreadActive)
			{
                using (var autoReleasePool = new NSAutoreleasePool())
                {
                    if (_motionManager.AccelerometerData != null)
                    {
                        OrientationChanged(_motionManager.AccelerometerData.Acceleration.X, _motionManager.AccelerometerData.Acceleration.Y, _motionManager.AccelerometerData.Acceleration.Z, (long)(_motionManager.AccelerometerData.Timestamp * 1000));
                    }

                    if (!_isOrientationUpdateThreadActive)
                    {
                        break;
                    }                    
                }

                Thread.Sleep(500);
			}
		}
    }
}