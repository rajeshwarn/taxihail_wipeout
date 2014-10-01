using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class LoadingOverlayView: UIView
    {
        private UIView _dialogView;
        private UIImageView _imageView;
        private CircularProgressView _progressView;
        private static float _dialogWidth = UIScreen.MainScreen.Bounds.Width;
        private static float _dialogHeight = 95;
        private static bool _isLoading;
        private UIWindow _modalWindow;

        public LoadingOverlayView()
        {
            BackgroundColor = UIColor.Black.ColorWithAlpha(0.75f);
            Frame = UIScreen.MainScreen.Bounds;

            _dialogView = new UIView();
            _dialogView.BackgroundColor = UIColor.White;

			var taxiProgressFile = "taxi_progress.png";
			var taxiProgressOverriden = IsTaxiAssetOverriden (taxiProgressFile, UIColor.FromRGBA (0, 122, 255, 255), new Point (10, 20) );
			var loadingImage = taxiProgressOverriden ? ImageHelper.GetImage(taxiProgressFile) : ImageHelper.ApplyThemeColorToImage (taxiProgressFile);
			_imageView = new UIImageView (loadingImage);
                
            _imageView.SizeToFit();
            _imageView.Hidden = true;

            _progressView = new CircularProgressView(new RectangleF(0, 0, 67, 67),  Theme.CompanyColor);
            _progressView.OnCompleted = () => Hide();
            _progressView.LineWidth = 1.5f;
            _progressView.Hidden = true;

            _dialogView.Frame  = new RectangleF(0, UIScreen.MainScreen.Bounds.Height / 2, UIScreen.MainScreen.Bounds.Width, 0);

            _progressView.SetHorizontalCenter(UIScreen.MainScreen.Bounds.Width / 2);
            _progressView.SetVerticalCenter(UIScreen.MainScreen.Bounds.Height / 2);

            _imageView.SetHorizontalCenter(UIScreen.MainScreen.Bounds.Width / 2);
            _imageView.SetVerticalCenter(UIScreen.MainScreen.Bounds.Height / 2);

            AddSubviews(_dialogView, _progressView, _imageView);
        }

        private void Animate()
        {
            var options = UIViewAnimationOptions.CurveEaseIn;

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
        private void IncreaseProgress()
        {
            var currentProgress = _progressView.Progress;

            var slowestSpeed = 0.00001f;

            // multistage progress speed
            if (currentProgress <= 0.2f)
            {
                _progressView.Progress += slowestSpeed * 50f;
                return;
            }

            if (currentProgress < 0.8f)
            {
                _progressView.Progress += slowestSpeed * 10f;
                return;
            }

            if (currentProgress > 0.8f)
            {
                var nextProgress = currentProgress + slowestSpeed;
                if (nextProgress >= 0.95f)
                {
                    nextProgress = 0.95f;
                }
                _progressView.Progress = nextProgress;
            }
        }

        public void Show()
        {
            _modalWindow =  _modalWindow ?? new UIWindow(UIScreen.MainScreen.Bounds);
            _modalWindow.Add(this);

            _modalWindow.MakeKeyAndVisible();

            _isLoading = true;
            Animate();

        }

        public void Dismiss()
        {
            // Will hide this view at the end of next animation cycle
            _isLoading = false;
        }

        private void Hide()
        {
            if (_modalWindow == UIApplication.SharedApplication.KeyWindow)
            {
                UIApplication.SharedApplication.Windows[0].MakeKeyWindow();
            }
            this.RemoveFromSuperview();
            _modalWindow.Hidden = true;
        }

        public bool IsTaxiAssetOverriden(string imagePath, UIColor expectedColor, Point expectedColorCoordinate)
        {
            var asset = ImageHelper.GetImage(imagePath);
            var detectedColor = asset.GetPixel(expectedColorCoordinate.X, expectedColorCoordinate.Y);
            return !detectedColor.CGColor.Equals(expectedColor.CGColor);
        }
    }
}

