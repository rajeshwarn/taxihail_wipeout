using System;
using CoreGraphics;
using Cirrious.MvvmCross.Touch.Views;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using Cirrious.CrossCore;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public abstract class BaseViewController<TViewModel> : MvxViewController, IHaveViewModel
        where TViewModel : PageViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;

        NSObject _applicationActivated;
        NSObject _applicationInBackground;

        private bool _firstStart = true;
        protected const float BottomPadding = 20f;

        public BaseViewController ()
        {
            Initialize ();
        }

        public BaseViewController(IntPtr handle) 
            : base(handle)
        {
            Initialize ();
        }

        protected BaseViewController(string nibName, NSBundle bundle) 
            : base(nibName, bundle)
        {
            Initialize ();
        }

        private void Initialize()
        {
            // Preserve iOS6 Behavior for compatibility reasons
            if (this.RespondsToSelector(new ObjCRuntime.Selector("automaticallyAdjustsScrollViewInsets")))
            {
                AutomaticallyAdjustsScrollViewInsets = false;
            }

            // To have the views under the nav bar and not under it
            if (this.RespondsToSelector(new ObjCRuntime.Selector("edgesForExtendedLayout")))
            {
                this.EdgesForExtendedLayout = UIRectEdge.Bottom;
            }
        }

        public new TViewModel ViewModel
        {
            get
            {
                return (TViewModel)DataContext;
            }
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if (ViewModel != null)
            {
                ViewModel.OnViewStopped();
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            Mvx.Resolve<IConnectivityService>().HandleToastInNewView();
            base.ViewDidAppear(animated);
            if (ViewModel != null)
            {
                ViewModel.OnViewStarted(_firstStart);
                _firstStart = false;
            }                
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            if (ViewModel != null)
            {
                ViewModel.OnViewUnloaded();
            }
        }
	
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // Setup keyboard event handlers
            RegisterForKeyboardNotifications ();

            RegisterForApplicationNotifications ();

            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);

            //remove gesture swipe to go back
            if (NavigationController != null)
            {
                NavigationController.InteractivePopGestureRecognizer.Enabled = false;
            }

            if (ViewModel != null)
            {
                ViewModel.OnViewLoaded();
            }  
        }        

        protected void DismissKeyboardOnReturn (params UITextField[] textFields)
        {
            if (textFields == null)
            {
                return;
            }

            foreach (var textField in textFields) 
            {
                textField.ReturnKeyType = UIReturnKeyType.Done;
                textField.ShouldReturn = ShouldReturn;
            }
        }

        private bool ShouldReturn (UITextField textField)
        {
            return textField.ResignFirstResponder();
        }

        protected virtual void RegisterForKeyboardNotifications ()
        {
            _keyboardObserverWillShow = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, KeyboardWillShowNotification);
            _keyboardObserverWillHide = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardWillHideNotification);
        }

        protected virtual void UnregisterKeyboardNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardObserverWillShow);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardObserverWillHide);
        }

        protected virtual void RegisterForApplicationNotifications()
        {
            _applicationActivated = NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.WillEnterForegroundNotification, OnActivated);
            _applicationInBackground = NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.DidEnterBackgroundNotification, DidEnterBackground);
        }

        protected virtual void UnregisterForApplicationNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_applicationActivated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_applicationInBackground);
        }

        protected virtual UIView KeyboardGetActiveView()
        {
            return View.FindFirstResponder();
        }

        protected virtual void OnActivated(NSNotification notification)
        {
        }

        protected virtual void DidEnterBackground(NSNotification notification)
        {
        }

        protected virtual void KeyboardWillShowNotification (NSNotification notification)
        {
            UIViewHelper.ReactToKeyboardWillShowNotification(this.View, KeyboardGetActiveView(), false, notification);
        }

        protected virtual void KeyboardWillHideNotification (NSNotification notification)
        {
            UIViewHelper.ReactToKeyboardWillHideNotification(this.View, KeyboardGetActiveView(), false, notification);
        }

        protected void ChangeThemeOfBarStyle()
        {
            // change color of status bar
            NavigationController.NavigationBar.BarStyle = Theme.IsLightContent
                ? UIBarStyle.Black
                : UIBarStyle.Default;
        }

        protected void ChangeThemeOfNavigationBar()
        {
            var titleFont = UIFont.FromName (FontName.HelveticaNeueMedium, 34/2);
            var navBarButtonFont = UIFont.FromName (FontName.HelveticaNeueLight, 34/2);

            var textColor = Theme.LabelTextColor;
            var navBarColor = Theme.CompanyColor;

            // change color of navigation bar
            NavigationController.NavigationBar.Translucent = false;
            UINavigationBar.Appearance.BarTintColor = navBarColor;
            NavigationController.NavigationBar.BarTintColor = navBarColor;

            // in ios7+, this is for the back arrow
            UIBarButtonItem.Appearance.TintColor = textColor;
            NavigationController.NavigationBar.TintColor = textColor;

            // change color of status bar
            ChangeThemeOfBarStyle ();

            // set title color
            var titleTextAttributes = new UITextAttributes
            {
                Font = titleFont,
                TextColor = textColor,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0, 0)
            };
            UINavigationBar.Appearance.SetTitleTextAttributes (titleTextAttributes); // for the first time the view is created

            var titleTextAttributesForNavBar = new UIStringAttributes
            {
                Font = titleFont,
                Shadow = new NSShadow
                {
                    ShadowColor = UIColor.Clear,
                    ShadowOffset = new CGSize()
                },
                ForegroundColor = textColor
            };
            NavigationController.NavigationBar.TitleTextAttributes = titleTextAttributesForNavBar; // when we return to a view, ensures the color has changed

            // set back/left/right button color
            var buttonTextColor = new UITextAttributes 
            {
                Font = navBarButtonFont,
                TextColor = textColor,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            };
            var selectedButtonTextColor = new UITextAttributes
            {
                Font = navBarButtonFont,
                TextColor = textColor.ColorWithAlpha(0.5f),
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            };

            UIBarButtonItem.Appearance.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);
        }

        protected void ChangeRightBarButtonFontToBold()
        {
            if (NavigationItem == null || NavigationItem.RightBarButtonItem == null)
            {
                return;
            }

            var rightBarButtonFont = UIFont.FromName (FontName.HelveticaNeueMedium, 34/2);
            var textColor = Theme.ButtonTextColor;

            var buttonTextColor = new UITextAttributes () {
                Font = rightBarButtonFont,
                TextColor = textColor,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            };
            var selectedButtonTextColor = new UITextAttributes () {
                Font = rightBarButtonFont,
                TextColor = textColor.ColorWithAlpha(0.5f),
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            };

            NavigationItem.RightBarButtonItem.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
            NavigationItem.RightBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
            NavigationItem.RightBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);
        }
    }
}

