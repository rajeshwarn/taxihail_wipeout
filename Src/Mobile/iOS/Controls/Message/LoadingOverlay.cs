using System;
using System.Drawing;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public static class LoadingOverlay
    {
        private static readonly object _lock = new object();
        private static int count = 0;
        private static UIView _modalBackgroundView;

        public static void StartAnimatingLoading()
        {
            lock (_lock)
            {
                count++;

                if (count == 1)
                {
                    _modalBackgroundView = new LoadingOverlayView();
                    UIApplication.SharedApplication.KeyWindow.AddSubview(_modalBackgroundView);
                }
            }
        }

        public static void StopAnimatingLoading()
        {
            lock (_lock)
            {
                if (count == 1)
                {
                    _modalBackgroundView.Dispose();
                    _modalBackgroundView = null;
                }

                count = Math.Max(0, --count);
            }
        }
    }
}
