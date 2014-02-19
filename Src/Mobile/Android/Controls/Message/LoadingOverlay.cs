using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.App;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class LoadingOverlay
    {
        private static ProgressDialog _progressBar;
        private static bool _isLoading;
        private static Activity _activity;

        public static void StartAnimatingLoading(Activity activity)
        {
            _activity = activity;

            _progressBar = new ProgressDialog(activity);
            _progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
            _progressBar.SetCancelable(false);
            _progressBar.Indeterminate = false;
            _progressBar.Progress = 0;
            _progressBar.SecondaryProgress = 0;
            _progressBar.Show();

            _isLoading = true;
            ThreadPool.QueueUserWorkItem(d =>
            {
                Animate();
            });
        }

        public static void StopAnimatingLoading()
        {
            if (_progressBar != null && _progressBar.IsShowing)
            {
                _isLoading = false;
            }
        }

        private static void Animate()
        {
            if (_progressBar != null)
            {
                if(_isLoading)
                {
                    IncreaseProgress();
                    Animate();
                }
                else
                {
                    _activity.RunOnUiThread(() =>
                    {
                        _progressBar.Progress = 100;
                    });

                    Thread.Sleep(500);

                    _activity.RunOnUiThread(() =>
                    {
                        _progressBar.Dismiss();
                    });
                }
            }
        }

        private static void IncrementProgress(int increment)
        {
            _activity.RunOnUiThread(() =>
            {
                _progressBar.IncrementProgressBy(increment);
            });
            Thread.Sleep(500);
        }

        private static void IncreaseProgress()
        {
            var currentProgress = _progressBar.Progress;

            var slowestSpeed = 1;

            // multistage progress speed
            if (currentProgress <= 20)
            {
                IncrementProgress(slowestSpeed * 3);
                return;
            }

            if (currentProgress < 80)
            {
                IncrementProgress(slowestSpeed * 2);
                return;
            }

            if (currentProgress > 80)
            {
                var nextProgress = currentProgress + slowestSpeed;
                if (nextProgress < 100)
                {
                    IncrementProgress(slowestSpeed);
                }
            }
        }
    }
}

