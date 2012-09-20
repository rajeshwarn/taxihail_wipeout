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
			AppContext.Current.Controller = toReturn;
            toReturn.NavigationBarHidden = false;
			LoadBackgroundNavBar(toReturn.NavigationBar);
            return toReturn;
        }

		public override void Close (Cirrious.MvvmCross.Interfaces.ViewModels.IMvxViewModel toClose)
		{
			base.Close (toClose);

		}

		protected override UIViewController CurrentTopViewController {
			get {
				return base.CurrentTopViewController;
			}
		}

		private void LoadBackgroundNavBar(UINavigationBar navBar)
        {
			navBar.TintColor = AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try
            {               
				navBar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch
            {
            }
        }

    }

}


