using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableChildView<TViewModel> : OverlayView
        where TViewModel : BaseViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;

        public BaseBindableChildView(IntPtr handle) : base(handle)
        {
            RegisterForKeyboardNotifications ();
        }

        public new TViewModel ViewModel
        {
            get
            {
				return (TViewModel)DataContext;
            }
        }

        protected void DismissKeyboardOnReturn (params UITextField[] textFields)
        {
            if (textFields == null)
                return;

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

        protected virtual UIView KeyboardGetActiveView()
        {
            return this.FindFirstResponder();
        }

        protected virtual void KeyboardWillShowNotification (NSNotification notification)
        {
            var activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;

            var scrollView = activeView.FindSuperviewOfType(this, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;

            // find the topmost scrollview (fix problem with RootElement)
            var nextSuperView = scrollView;
            while(nextSuperView != null)
            {
                scrollView = nextSuperView;
                nextSuperView = scrollView.FindSuperviewOfType(this, typeof(UIScrollView)) as UIScrollView;
            }

            var keyboardBounds = ((NSValue)notification.UserInfo.ValueForKey(UIKeyboard.FrameEndUserInfoKey)).RectangleFValue;

            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardBounds.Size.Height + this.Superview.Frame.Y, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;

            // If activeField is hidden by keyboard, scroll it so it's visible
            var viewRectAboveKeyboard = new RectangleF(this.Superview.Frame.Location, new SizeF(this.Superview.Frame.Width, this.Superview.Frame.Size.Height - keyboardBounds.Size.Height));

            var activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, this.Superview);
            // activeFieldAbsoluteFrame is relative to this.View so does not include any scrollView.ContentOffset
            activeFieldAbsoluteFrame.Y = activeFieldAbsoluteFrame.Y + this.Superview.Frame.Y;

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

            var scrollView = activeView.FindSuperviewOfType (this, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;

            // find the topmost scrollview (fix problem with RootElement)
            var nextSuperView = scrollView;
            while(nextSuperView != null)
            {
                scrollView = nextSuperView;
                nextSuperView = scrollView.FindSuperviewOfType(this.Superview, typeof(UIScrollView)) as UIScrollView;
            }

            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            var animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, 0.0f, 0.0f);
            UIView.Animate(animationDuration, delegate{
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            });
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterKeyboardNotifications();
            base.Dispose(disposing);
        }
    }
}

