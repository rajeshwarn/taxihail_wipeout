
using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views.Cmt
{
    public partial class CmtHomeView : MvxBindingTouchViewController<CmtHomeViewModel>, INavigationView
    {
        private PanelMenuView _menu;

        public CmtHomeView () 
            : base(new MvxShowViewModelRequest<CmtHomeViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
            
        }
        
        public CmtHomeView (MvxShowViewModelRequest request) 
            : base(request)
        {
            
        }
        
        public CmtHomeView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
            
        }

        #region INavigationView implementation
        
        public bool HideNavigationBar {
            get { return true;}
        }
        
        #endregion

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            navBar.SetBackgroundImage (UIImage.FromFile ("Assets/navBar.png"), UIBarMetrics.Default);
            navBar.TopItem.TitleView = new TitleView (null, "", false);
            
            homeView.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            this.View.ApplyAppFont ();
            ViewModel.Load();
            
            _menu = new PanelMenuView (homeView, this.NavigationController, ViewModel.Panel);
            View.InsertSubviewBelow (_menu.View, homeView);
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);     
            
            NavigationController.NavigationBar.Hidden = true;            
            
        }
        
        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            
            var button = AppButtons.CreateStandardButton( new RectangleF( 16,0,40,33 ) , "", AppStyle.ButtonColor.Black, "Assets/settings.png");
            button.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;
            var offsetView = new UIView( new RectangleF( 0,0,60,33) );
            offsetView.AddSubview ( button );
            
            var btn = new UIBarButtonItem ( offsetView );
            navBar.TopItem.RightBarButtonItem = btn;
            navBar.TopItem.RightBarButtonItem.SetTitlePositionAdjustment (new UIOffset (-20, 0), UIBarMetrics.Default);
        }
    }
}

