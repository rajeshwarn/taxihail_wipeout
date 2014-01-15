using System.Drawing;
using System.Linq;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter: MvxModalSupportTouchViewPresenter
    {
       public PhonePresenter(UIApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }

        protected override UINavigationController CreateNavigationController(UIViewController viewController)
        {
            var navController = new UINavigationController(viewController);
            Mvx.RegisterSingleton<UINavigationController>(navController);

            return navController;
        }
    }
}


