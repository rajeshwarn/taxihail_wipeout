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
        const double radiansToDegrees = 360 / (2 * Math.PI);
        const double accelerometerUpdateInterval = 1 / 5; // 5 Hz

        CMMotionManager motionManager;
        NSOperationQueue accelerometerUpdateQueue;

        public AppleDeviceOrientationService()
        {
            if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.DEVICE)
                motionManager = new CMMotionManager();
        }

        public override bool IsAvailable()
        {
            if (motionManager != null)
                return motionManager.DeviceMotionAvailable && motionManager.AccelerometerAvailable;
            else
                return false;
        }

        protected override bool StartService()
        {
            if (motionManager != null && IsAvailable())
            {
                accelerometerUpdateQueue = new NSOperationQueue();
                motionManager.AccelerometerUpdateInterval = accelerometerUpdateInterval;
                motionManager.StartAccelerometerUpdates(accelerometerUpdateQueue, AngleChangedEvent);
                return true;
            }

            return false;
        }

        protected override bool StopService()
        {
            if (motionManager != null && IsAvailable())
            {
                motionManager.StopAccelerometerUpdates();
                return true;
            }

            return true;
        }

        void AngleChangedEvent(CMAccelerometerData data, NSError error)
        {
            if (error == null)
            {
                if ((data.Acceleration.X * data.Acceleration.X + data.Acceleration.Y * data.Acceleration.Y) * 4 >= data.Acceleration.Z * data.Acceleration.Z)
                {
                    int angle = (int)Math.Round(Math.Atan2(data.Acceleration.Y, -data.Acceleration.X) * radiansToDegrees) - 270;

                    while (angle >= 360)
                        angle -= 360;

                    while (angle < 0)
                        angle += 360;

                    AngleChangedEvent(angle);
                }
            }
        }
    }
}