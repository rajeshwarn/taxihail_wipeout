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
    public class LoadingOverlay : UIView
    {
        private static UIView _modalBackgroundView;
        private static UIView _dialogView;
        private static CircularProgressView _progressView;
        private static UIImageView _imageView;

        private static float _dialogWidth = UIScreen.MainScreen.Bounds.Width;
        private static float _dialogHeight = 95;
        private static bool _isLoading;

        private static readonly object _lock = new object();

        public static void StartAnimatingLoading()
        {
            lock (_lock)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    if (_modalBackgroundView == null)
                    {
                        _modalBackgroundView = new UIView();
                        _modalBackgroundView.BackgroundColor = UIColor.Black.ColorWithAlpha(0.75f);

                        _dialogView = new UIView();
                        _dialogView.BackgroundColor = UIColor.White;

                        if(_imageView == null)
                        {
                            _imageView = new UIImageView(UIImage.FromFile("taxi_progress.png"));
                            _imageView.SizeToFit();
                            _imageView.Hidden = true;
                        }

                        _progressView = new CircularProgressView(new RectangleF(0, 0, 67, 67));
                        _progressView.OnCompleted = () => CloseOverlay();
                        _progressView.LineWidth = 1.5f;
                        _progressView.Hidden = true;
                    }

                    _modalBackgroundView.Frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);

                    _dialogView.Frame  = new RectangleF(0, UIScreen.MainScreen.Bounds.Height / 2, UIScreen.MainScreen.Bounds.Width, 0);
                    _modalBackgroundView.AddSubview(_dialogView);

                    _progressView.SetHorizontalCenter(UIScreen.MainScreen.Bounds.Width / 2);
                    _progressView.SetVerticalCenter(UIScreen.MainScreen.Bounds.Height / 2);
                    _modalBackgroundView.AddSubview(_progressView);

                    _imageView.SetHorizontalCenter(UIScreen.MainScreen.Bounds.Width / 2);
                    _imageView.SetVerticalCenter(UIScreen.MainScreen.Bounds.Height / 2);
                    _modalBackgroundView.AddSubview(_imageView);

                    UIApplication.SharedApplication.KeyWindow.AddSubview(_modalBackgroundView);

                    _isLoading = true;
                    Animate();
                });
            }
        }

        private static void Animate()
        {
            var options = UIViewAnimationOptions.CurveEaseIn;

            if (_dialogView != null)
            {
                UIView.Animate(
                    0.2, 0, options, 
                    () =>
                    {
                        _dialogView.Frame = new RectangleF(0, (UIScreen.MainScreen.Bounds.Height - _dialogHeight) / 2, _dialogWidth, _dialogHeight);
                    },
                    () => 
                    {
                        _progressView.Hidden = false;
                        _imageView.Hidden = false;

                        if(_isLoading)
                        {
                            IncreaseProgress();
                            Animate();
                        }
                        else
                        {
                            _progressView.Progress = 1f;
                        }
                    });
            }
        }

        private static void IncreaseProgress()
        {
            var currentProgress = _progressView.Progress;

            var slowestSpeed = 0.00001f;

            // multistage progress speed
            if (currentProgress <= 0.2f)
            {
                _progressView.Progress += slowestSpeed * 25f;
                return;
            }

            if (currentProgress < 0.8f)
            {
                _progressView.Progress += slowestSpeed * 5.5f;
                return;
            }

            if (currentProgress > 0.8f)
            {
                var nextProgress = currentProgress + slowestSpeed;
                if (nextProgress >= 1.0f)
                {
                    nextProgress = 0.99999f;
                }
                _progressView.Progress = nextProgress;
            }
        }

        public static void StopAnimatingLoading()
        {
            lock (_lock)
            {
                if (_modalBackgroundView != null)
                {
                    _isLoading = false;
                }
            }
        }

        private static void CloseOverlay()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() => 
            {
                _dialogView.RemoveFromSuperview();
                _dialogView = null;

                _modalBackgroundView.RemoveFromSuperview();
                _modalBackgroundView = null;

                _progressView.RemoveFromSuperview();
                _progressView = null;

                _imageView.RemoveFromSuperview();
                _imageView = null;
            });
        }

        public LoadingOverlay(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public LoadingOverlay(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public LoadingOverlay() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}
