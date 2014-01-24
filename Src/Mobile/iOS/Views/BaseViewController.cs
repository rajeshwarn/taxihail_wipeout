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
		
        public void ApplyThemeToNavigationBar()
        {
            var textColor = Theme.IsLightContent 
                            ? UIColor.White
                            : UIColor.FromRGB(44, 44, 44) ;

            // navigation bar
            if (UIHelper.IsOS7orHigher) {
                NavigationController.NavigationBar.BarTintColor = Theme.BackgroundColor;
                NavigationController.NavigationBar.TintColor = textColor; //in ios7, this is for the back chevron
            } else {
                NavigationController.NavigationBar.TintColor = Theme.BackgroundColor; //in ios6, this is for the bar color

                //change the default ios6 back button look to the ios7 look
                var clearBackground = UIImage.FromFile ("clearButton.png").CreateResizableImage(UIEdgeInsets.Zero);
                var backBackground = UIImage.FromFile (Theme.IsLightContent ? "left_arrow_white.png" : "left_arrow.png").CreateResizableImage (new UIEdgeInsets (0, 12, 21, 0));

                NavigationItem.BackBarButtonItem.SetBackgroundImage(clearBackground, UIControlState.Normal, UIBarMetrics.Default);
                NavigationItem.BackBarButtonItem.SetBackButtonBackgroundImage(backBackground, UIControlState.Normal, UIBarMetrics.Default);

                if (NavigationItem.LeftBarButtonItem != null)
                {
                    NavigationItem.LeftBarButtonItem.SetBackgroundImage(clearBackground, UIControlState.Normal, UIBarMetrics.Default);
                    NavigationItem.LeftBarButtonItem.SetBackButtonBackgroundImage(backBackground, UIControlState.Normal, UIBarMetrics.Default);
                }

                if (NavigationItem.RightBarButtonItem != null)
                {
                    NavigationItem.RightBarButtonItem.SetBackgroundImage(clearBackground, UIControlState.Normal, UIBarMetrics.Default);
                    NavigationItem.RightBarButtonItem.SetBackButtonBackgroundImage(backBackground, UIControlState.Normal, UIBarMetrics.Default);
                }
            }

            var titleFont = UIFont.FromName (FontName.HelveticaNeueMedium, 34/2);
            var navBarButtonFont = UIFont.FromName (FontName.HelveticaNeueLight, 34/2);

            NavigationController.NavigationBar.SetTitleTextAttributes (new UITextAttributes () {
                TextColor = textColor,
                Font = titleFont,
                TextShadowColor = UIColor.Clear,
                TextShadowOffset = new UIOffset(0,0)
            });

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

            NavigationItem.BackBarButtonItem.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
            NavigationItem.BackBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
            NavigationItem.BackBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);

            if (NavigationItem.LeftBarButtonItem != null)
            {
                NavigationItem.LeftBarButtonItem.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
                NavigationItem.LeftBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
                NavigationItem.LeftBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);
            }

            if (NavigationItem.RightBarButtonItem != null)
            {
                NavigationItem.RightBarButtonItem.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
                NavigationItem.RightBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
                NavigationItem.RightBarButtonItem.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);
            }
        }
    }
}

