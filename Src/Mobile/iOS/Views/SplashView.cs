using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class SplashView : UIViewController
    {
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.White;

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
    }
}

