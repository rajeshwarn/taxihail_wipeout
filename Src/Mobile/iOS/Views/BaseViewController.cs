
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client
{
    public abstract class BaseViewController<TViewModel> : MvxBindingTouchViewController<TViewModel>, IHaveViewModel where TViewModel: BaseViewModel, IMvxViewModel
    {
        NSObject _keyboardObserverWillShow;
        NSObject _keyboardObserverWillHide;
        private bool _firstStart = true;

		#region IHaveViewModel implementation

		public BaseViewModel MyViewModel {
			get {
				return ViewModel;
			}
		}

		#endregion

        #region Constructors

        public BaseViewController () 
            : base(new MvxShowViewModelRequest<TViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public BaseViewController(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BaseViewController(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
#endregion

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            if (_firstStart) {
                _firstStart = false;
                ViewModel.Start (firstStart: true);
            } else {
                ViewModel.Restart();
                ViewModel.Start();
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

            Background.LoadForRegularView (this.View);
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Resources.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
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
            return this.View.FindFirstResponder();
        }
        
        protected virtual void KeyboardWillShowNotification (NSNotification notification)
        {
            UIView activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
            UIScrollView scrollView = activeView.FindSuperviewOfType(this.View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;
            
            RectangleF keyboardBounds = UIKeyboard.BoundsFromNotification(notification);
            
            UIEdgeInsets contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardBounds.Size.Height, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;
            
            // If activeField is hidden by keyboard, scroll it so it's visible
            RectangleF viewRectAboveKeyboard = new RectangleF(this.View.Frame.Location, new SizeF(this.View.Frame.Width, this.View.Frame.Size.Height - keyboardBounds.Size.Height));
            
            RectangleF activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, this.View);
            // activeFieldAbsoluteFrame is relative to this.View so does not include any scrollView.ContentOffset
			activeFieldAbsoluteFrame.Y = activeFieldAbsoluteFrame.Y + this.View.Frame.Y;

            // Check if the activeField will be partially or entirely covered by the keyboard
            if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
            {
                // Scroll to the activeField Y position + activeField.Height + current scrollView.ContentOffset.Y - the keyboard Height
                PointF scrollPoint = new PointF(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + scrollView.ContentOffset.Y - viewRectAboveKeyboard.Height);
                scrollView.SetContentOffset(scrollPoint, true);
            }
        }
        
        protected virtual void KeyboardWillHideNotification (NSNotification notification)
        {
            UIView activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
            UIScrollView scrollView = activeView.FindSuperviewOfType (this.View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;
            
            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            double animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
            UIEdgeInsets contentInsets = new UIEdgeInsets(0.0f, 0.0f, 0.0f, 0.0f);
            UIView.Animate(animationDuration, delegate{
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            });
        }
		
    }
}

