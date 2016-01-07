using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ToastView : UIView
    {
        private string _message;
        private UIWindow _modalWindow;
        private UILabel _messageView;
        private const float _dialogEdgeConstraint = 10f;
        private const float _dialogTopEdgeConstraint = 3f;
        private const float _toastHeight = 65f;

        public ToastView(string message)
        {
            _message = message;

            InitToastView();
        }

        public async void Show()
        {
            _modalWindow = _modalWindow ?? new UIWindow(UIScreen.MainScreen.Bounds) 
                { 
                    RootViewController = new UIViewController()
                };

            _modalWindow.MakeKeyAndVisible();
            _modalWindow.RootViewController.View.AddSubview(this);

            await UIView.AnimateAsync(0.3, () =>
                {
                    this.Frame = new CGRect(0, UIScreen.MainScreen.Bounds.Height-_toastHeight, UIScreen.MainScreen.Bounds.Width, _toastHeight);
                });
        }

        public async void Dismiss()
        {
            if (_modalWindow == UIApplication.SharedApplication.KeyWindow)
            {
                UIApplication.SharedApplication.Windows[0].MakeKeyWindow();
            }

            await UIView.AnimateAsync(0.3, () =>
                {
                    this.Frame = new CGRect(0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width, _toastHeight);
                });

            this.RemoveFromSuperview();
            _modalWindow.Hidden = true;
            LayoutIfNeeded();
        }

        private void InitToastView()
        {
            Frame = new CGRect(0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width, _toastHeight);

            BackgroundColor = Theme.LoginColor;

            _messageView = new UILabel
                {
                    Text = _message,
                    Font = UIFont.SystemFontOfSize(16),
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextAlignment = UITextAlignment.Center,
                    Lines = 0,
                    TextColor = Theme.GetContrastBasedColor(Theme.LoginColor)
                };

            this.Add(_messageView); 

            this.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, _dialogTopEdgeConstraint),
                    NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                    NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                });
        }
    }
}

