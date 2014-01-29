using System.Drawing;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.UIKit;
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

        public override void Show(MvxViewModelRequest request)
        {
            base.Show(request);
            if(request.ParameterValues != null
                && request.ParameterValues.ContainsKey("removeFromHistory"))
            {
                RemovePreviousViewFromHistory();
            }
        }

        private void RemovePreviousViewFromHistory()
        { 
            var navController = Mvx.Resolve<UINavigationController>();

            var controllers = navController.ViewControllers;
            if (controllers.Length > 1)
            {
                var listOfControllers = new List<UIViewController>(controllers);
                listOfControllers.RemoveAt(listOfControllers.Count - 2);
                navController.ViewControllers = listOfControllers.ToArray();
            }
            else
            {
                Mvx.Warning("Can't remove previous view, not enough UIViewControllers in the stack");
            }

        }
    }
}


