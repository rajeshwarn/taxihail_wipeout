using System;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter: MvxModalSupportTouchViewPresenter
    {
        private UINavigationController _masterNavigationController;

        public PhonePresenter(UIApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }
        
        protected override UINavigationController CreateNavigationController(UIViewController viewController)
        {
            _masterNavigationController= base.CreateNavigationController(viewController);
            AppContext.Current.Controller = _masterNavigationController;
            _masterNavigationController.NavigationBarHidden = false;


            LoadFakeNavigationBackground();
            LoadBackgroundNavBar(_masterNavigationController.NavigationBar);
            return _masterNavigationController;
        }

        //Sometime we cannot show the navigation bar.  We need to load the fake background to avoid seeing a white background on top and at bottom
        private void LoadFakeNavigationBackground()
        {
            var navBar = new UIImageView();
            navBar.Image = UIImage.FromFile( "Assets/navBar.png" );
            navBar.Frame = new System.Drawing.RectangleF( 0, 20,320,44);
            _masterNavigationController.View.InsertSubview(  navBar , 0 );

            var back = new UIImageView();
            back.Image = UIImage.FromFile( "Assets/background.png" );
            back.Frame = new System.Drawing.RectangleF( 0, 64,320, back.Image.Size.Height);
            _masterNavigationController.View.InsertSubview(  back , 0 );

        }

       
        public override void Show(Cirrious.MvvmCross.Touch.Interfaces.IMvxTouchView view)
        {        
            base.Show(view);                      
            ((UIViewController)view).NavigationController.NavigationBar.Hidden = HideNavBar(view);
        }

        private bool HideNavBar(Cirrious.MvvmCross.Touch.Interfaces.IMvxTouchView view)
        {
            return ( view is INavigationView ) && (((INavigationView)view).HideNavigationBar );
        }

      
        public override bool PresentModalViewController (UIViewController viewController, bool animated)
        {
            CurrentTopViewController.NavigationController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext; 
            return base.PresentModalViewController (viewController, animated);
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


