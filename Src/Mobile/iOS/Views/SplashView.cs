using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class SplashView : UIViewController
    {
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            var imageView = new UIImageView(GetSplashImage());
            View.AddSubview(imageView);

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
            //TODO needs to be fixed when migrating to XCode6
            if (UIHelper.Is4InchDisplay)
            {
                return UIImage.FromBundle("Default-568h");
            }

            return UIImage.FromBundle("Default");
        }
    }
}

