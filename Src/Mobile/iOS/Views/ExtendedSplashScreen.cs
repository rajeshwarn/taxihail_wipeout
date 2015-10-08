using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class ExtendedSplashScreenView : BaseViewController<ExtendedSplashScreenViewModel>
    {
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            NavigationController.NavigationBarHidden = true;

            var splashImage = GetSplashImage();
            if (splashImage != null)
            {
                var imageView = new UIImageView(splashImage);
                View.AddSubview(imageView);
            }

            var activityIndicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge) 
            {
                Color = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            activityIndicator.StartAnimating();
            View.Add(activityIndicator);

            View.AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(activityIndicator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, activityIndicator.Superview, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(activityIndicator, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, activityIndicator.Superview, NSLayoutAttribute.CenterX, 1, 0),
            });
        }

        private UIImage GetSplashImage()
        {
            // LaunchImage doesn't correctly work as an asset catalog
            // We have to use the standardized names given by Apple

            var backgroundImage = UIImage.FromBundle ("LaunchImage");

            if (UIScreen.MainScreen.Bounds.Height == 568)
            {
                backgroundImage = UIImage.FromBundle("LaunchImage-568h");
            }
            else if (UIScreen.MainScreen.Bounds.Height == 667)
            {
                backgroundImage = UIImage.FromBundle("LaunchImage-800-667h");
            }
            else if (UIScreen.MainScreen.Bounds.Height == 736)
            {
                backgroundImage = UIImage.FromBundle ("LaunchImage-800-Portrait-736h");
            }
                
            return backgroundImage;
        }
    }
}

