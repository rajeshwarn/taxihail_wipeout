using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public abstract class BaseViewController<TViewModel> : MvxViewController, IHaveViewModel
		where TViewModel : BaseViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;
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
			if (this.RespondsToSelector(new MonoTouch.ObjCRuntime.Selector("automaticallyAdjustsScrollViewInsets")))
			{
				AutomaticallyAdjustsScrollViewInsets = false;
			}

			// To have the views under the nav bar and not under it
			if (this.RespondsToSelector(new MonoTouch.ObjCRuntime.Selector("edgesForExtendedLayout")))
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

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            ViewModel.OnViewStarted (_firstStart);
            _firstStart = false;
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if(ViewModel!= null) ViewModel.OnViewStopped();
        }
				
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            // Setup keyboard event handlers
            RegisterForKeyboardNotifications ();

            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
        }
		
        public override void DidReceiveMemoryWarning ()
        {
            UnregisterKeyboardNotifications();
            ViewModel.OnViewUnloaded ();
            base.DidReceiveMemoryWarning ();
        }

        protected void DismissKeyboardOnReturn (params UITextField[] textFields)
        {
            if (textFields == null)
                return;
            foreach (var textField in textFields) {
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
        
        protected virtual UIView KeyboardGetActiveView()
        {
            return View.FindFirstResponder();
        }
        
        protected virtual void KeyboardWillShowNotification (NSNotification notification)
        {
            var activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
			var scrollView = activeView.FindSuperviewOfType(View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;
            
			// find the topmost scrollview (fix problem with RootElement)
			var nextSuperView = scrollView;
			while(nextSuperView != null)
			{
				scrollView = nextSuperView;
				nextSuperView = scrollView.FindSuperviewOfType(this.View, typeof(UIScrollView)) as UIScrollView;
			}

			var keyboardBounds = ((NSValue)notification.UserInfo.ValueForKey(UIKeyboard.FrameEndUserInfoKey)).RectangleFValue;

			var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardBounds.Size.Height + this.View.Frame.Y, 0.0f);
			scrollView.ContentInset = contentInsets;
			scrollView.ScrollIndicatorInsets = contentInsets;

			// If activeField is hidden by keyboard, scroll it so it's visible
			var viewRectAboveKeyboard = new RectangleF(this.View.Frame.Location, new SizeF(this.View.Frame.Width, this.View.Frame.Size.Height - keyboardBounds.Size.Height));

			var activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, this.View);
			// activeFieldAbsoluteFrame is relative to this.View so does not include any scrollView.ContentOffset
			activeFieldAbsoluteFrame.Y = activeFieldAbsoluteFrame.Y + this.View.Frame.Y;

			// Check if the activeField will be partially or entirely covered by the keyboard
			if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
			{
				// Scroll to the activeField Y position + activeField.Height + current scrollView.ContentOffset.Y - the keyboard Height
				var scrollPoint = new PointF(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + scrollView.ContentOffset.Y - viewRectAboveKeyboard.Height);
				scrollView.SetContentOffset(scrollPoint, true);
			}
        }
        
        protected virtual void KeyboardWillHideNotification (NSNotification notification)
        {
            var activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
            var scrollView = activeView.FindSuperviewOfType (View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;
            
			// find the topmost scrollview (fix problem with RootElement)
			var nextSuperView = scrollView;
			while(nextSuperView != null)
			{
				scrollView = nextSuperView;
				nextSuperView = scrollView.FindSuperviewOfType(this.View, typeof(UIScrollView)) as UIScrollView;
			}

            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            var animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, 0.0f, 0.0f);
            UIView.Animate(animationDuration, delegate{
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            });
        }

        protected void ChangeThemeOfNavigationBar(bool resetToDefault = false)
        {
            var titleFont = UIFont.FromName (FontName.HelveticaNeueMedium, 34/2);
            var navBarButtonFont = UIFont.FromName (FontName.HelveticaNeueLight, 34/2);

            //is false since now we are using the theme color in navbar even when logged in
            var isDefaultColor = false;

            var textColor = isDefaultColor
                                ? Theme.LabelTextColor
                                : UIColor.White;

            var navBarColor = isDefaultColor
                              ? UIColor.White
                              : Theme.BackgroundColor;

            var isOS7orHigher = UIHelper.IsOS7orHigher;

            // change color of status bar
            if (isOS7orHigher)
            {
                NavigationController.NavigationBar.BarStyle = isDefaultColor
                                                              ? UIBarStyle.Default
                                                              : UIBarStyle.Black;
            }

            // change color of navigation bar
            if (isOS7orHigher) 
            {
                UINavigationBar.Appearance.BarTintColor = navBarColor;
                NavigationController.NavigationBar.BarTintColor = navBarColor;

                // in ios7, this is for the back arrow, in ios6, it's for the color of the bar.  this is why we have to put it in this if block
                UIBarButtonItem.Appearance.TintColor = textColor;
                NavigationController.NavigationBar.TintColor = textColor;
            } 
            else 
            {
                // change the background color of the nav bar (setting an image gets rid of the gradient look)
                UIImage clearBackground;
                if (isDefaultColor)
                {
                    clearBackground = UIImage.FromFile("clearButton.png");
                }
                else
                {
                    clearBackground = ImageHelper.CreateFromColor(navBarColor);
                }
                clearBackground = clearBackground.CreateResizableImage(UIEdgeInsets.Zero);
                UINavigationBar.Appearance.SetBackgroundImage(clearBackground, UIBarMetrics.Default); 
                NavigationController.NavigationBar.SetBackgroundImage(clearBackground, UIBarMetrics.Default);

                //change the default ios6 back button look to the ios7 look
                var backBackground = UIImage.FromFile (isDefaultColor ? "left_arrow.png" : "left_arrow_white.png").CreateResizableImage (new UIEdgeInsets (0, 12, 21, 0));
                UIBarButtonItem.Appearance.SetBackgroundImage(clearBackground, UIControlState.Normal, UIBarMetrics.Default); 
                UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(backBackground, UIControlState.Normal, UIBarMetrics.Default); 
            }

            // set title color
            var titleTextAttributes = new UITextAttributes()
            {
                Font = titleFont,
                TextColor = textColor,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0, 0)
            };
            UINavigationBar.Appearance.SetTitleTextAttributes (titleTextAttributes); // for the first time the view is created
            NavigationController.NavigationBar.SetTitleTextAttributes (titleTextAttributes); // when we return to a view, ensures the color has changed

            // set back/left/right button color
            var buttonTextColor = new UITextAttributes () {
                Font = navBarButtonFont,
                TextColor = textColor,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            };
            var selectedButtonTextColor = new UITextAttributes () {
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
            var textColor = UIColor.White;

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

