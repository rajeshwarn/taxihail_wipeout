using System;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter: MvxModalSupportTouchViewPresenter
    {
        public PhonePresenter(UIApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }
        
        protected override UINavigationController CreateNavigationController (UIViewController viewController)
        {
            var toReturn = base.CreateNavigationController (viewController);
            toReturn.NavigationBarHidden = false;
            return toReturn;
        }

    }

}


