using System.Drawing;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Navigation;
using System.Collections.Generic;

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

        public override void ChangePresentation(MvxPresentationHint hint)
        {           
            if(hint is ClearHistoryPresentationHint)
            {
                var navController = Mvx.Resolve<UINavigationController>();

                var controllers = navController.ViewControllers;
                var listOfControllers = new List<UIViewController>(controllers);
                listOfControllers.RemoveAt(listOfControllers.Count - 2);

                navController.ViewControllers = listOfControllers.ToArray();
            }else
            {
                base.ChangePresentation(hint);
            }
        }
    }
}


