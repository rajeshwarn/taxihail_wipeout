using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.ObjCRuntime;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public abstract class BaseViewController<TViewModel> : MvxViewController, IHaveViewModel
		where TViewModel : BaseViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;
        private bool _firstStart = true;
		protected const float BottomPadding = 20f;

        #region Constructors

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
        
#endregion

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
            if (_firstStart) {
                _firstStart = false;
                ViewModel.Start (firstStart: true);
            } else {
                ViewModel.Restart();
                ViewModel.Start(firstStart: false);
            }
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if(ViewModel!= null) ViewModel.Stop();
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
            ViewModel.Unload ();
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
            
            var keyboardBounds = UIKeyboard.FrameBeginFromNotification(notification);
            
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardBounds.Size.Height, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;
            
            // If activeField is hidden by keyboard, scroll it so it's visible
            var viewRectAboveKeyboard = new RectangleF(View.Frame.Location, new SizeF(View.Frame.Width, View.Frame.Size.Height - keyboardBounds.Size.Height));
            
            var activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, View);
            // activeFieldAbsoluteFrame is relative to this.View so does not include any scrollView.ContentOffset
			activeFieldAbsoluteFrame.Y = activeFieldAbsoluteFrame.Y + View.Frame.Y;

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
            
            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            var animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, 0.0f, 0.0f);
            UIView.Animate(animationDuration, delegate{
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            });
        }
		
    }
}

