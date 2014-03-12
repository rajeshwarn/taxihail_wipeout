using System;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public static class LoadingOverlay
    {
        private static readonly object _lock = new object();
        private static int count = 0;
        private static LoadingOverlayView _modalBackgroundView;

        public static UIView StartAnimatingLoading()
        {
            lock (_lock)
            {
                count++;

                if (count == 1)
                {
                    _modalBackgroundView = new LoadingOverlayView();
                    _modalBackgroundView.Show();

                }
            }
            return _modalBackgroundView;
        }

        public static void StopAnimatingLoading()
        {
            lock (_lock)
            {
                if (count == 1)
                {
                    _modalBackgroundView.Dismiss();
                    _modalBackgroundView = null;
                }

                count = Math.Max(0, count-1);
            }
        }
    }
}
