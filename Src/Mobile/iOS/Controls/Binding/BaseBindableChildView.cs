using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using UIKit;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableChildView<TViewModel> : OverlayView
        where TViewModel : BaseViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;

		public BaseBindableChildView():base()
		{
			RegisterForKeyboardNotifications ();
		}

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
            UIViewHelper.ReactToKeyboardWillShowNotification(this, KeyboardGetActiveView(), true, notification);
        }

        protected virtual void KeyboardWillHideNotification (NSNotification notification)
        {
            UIViewHelper.ReactToKeyboardWillHideNotification(this, KeyboardGetActiveView(), true, notification);
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterKeyboardNotifications();
            base.Dispose(disposing);
        }
    }
}

