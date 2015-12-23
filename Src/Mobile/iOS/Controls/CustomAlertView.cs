using System;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using ObjCRuntime;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;


namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class CustomAlertView: UIView
    {
        private UIView _dialogView;
        private float _dialogHeight;
        private const float _dialogEdgeConstraint = 10f;
        private const float _buttonsHeight = 35f;
        private const float _globalMargin = 20f;
        private const float _dialogMargin = 5f;
        private UIWindow _modalWindow;

        private UILabel _messageView;

        private bool _hasTitle = true;
        private bool _isPrompt = false;
        private string _message;
        private string _title;

        public CustomAlertView(string title, string message, string cancelButtonTitle)
        {
            _hasTitle = !string.IsNullOrEmpty(title);
            _title = title;
            _message = message;
            InitDialogView();
            InitCancelButton(cancelButtonTitle);
        }
        public CustomAlertView(string title, string message)
        {
            _hasTitle = !string.IsNullOrEmpty(title);
            _title = title;
            _message = message;
            InitDialogView();
            InitCancelButton(Localize.GetValue("OkButtonText"));
        }

        public CustomAlertView(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
            _hasTitle = !string.IsNullOrEmpty(title);
            _title = title;
            _message = message;
            InitDialogView();
            InitDoubleButtons(negativeButtonTitle, positiveButtonTitle,positiveAction, negativeAction, _messageView);
        }

        public CustomAlertView(string title, string message, string positiveButtonTitle, 
            Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            _hasTitle = !string.IsNullOrEmpty(title);
            _title = title;
            _message = message; 
            InitDialogView();
            InitTrippleButtons(negativeButtonTitle, positiveButtonTitle, neutralButtonTitle, negativeAction, positiveAction, neutralAction);
        }

        public CustomAlertView(string title, string message, Action cancelAction, bool isNumericOnly = false, string inputText = "")
        {
            _title = title;
            _message = message;
            InitPrompt(cancelAction, isNumericOnly, inputText);
        }

        public UITextView CustomInputView{ get; set;}
        public EventHandler Dismissed { get; set;}
        public EventHandler Clicked { get; set;}

        public bool CanBeDismissed { get; set;}

        public void Show()
        {
            _modalWindow = _modalWindow ?? new UIWindow(UIScreen.MainScreen.Bounds) 
            { 
                WindowLevel = UIWindowLevel.Alert,
                RootViewController = new UIViewController()
            };

            _modalWindow.MakeKeyAndVisible();
            _modalWindow.RootViewController.View.AddSubview(this);

            AnimateShow();
        }

        private void InitDialogView()
        {
            Frame = UIScreen.MainScreen.Bounds;

            BackgroundColor = UIColor.Black.ColorWithAlpha(0.35f);

            _dialogView = new UIView(new CGRect(0, 0, Frame.Width - (_dialogMargin * 2), 0))
                {
                    BackgroundColor = Theme.LoginColor,
                    Center = this.Center,
                    Alpha = 0,
                };

             _dialogView.Layer.CornerRadius = 10;

            var title = new UILabel()
                {
                    Text = _title,
                    Font = UIFont.BoldSystemFontOfSize(16),
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextAlignment = UITextAlignment.Center,
                    Lines = 0,
                    TextColor = Theme.GetContrastBasedColor(Theme.LoginColor)
                };

            if (_hasTitle)
            {
                _dialogView.Add(title); 
                _dialogView.AddConstraints(new [] 
                    {
                        NSLayoutConstraint.Create(title, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Top, 1f, _dialogEdgeConstraint),
                        NSLayoutConstraint.Create(title, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                        NSLayoutConstraint.Create(title, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                    });
            }

            _messageView = new UILabel()
            {
                Text = _message,
                Font = UIFont.SystemFontOfSize(16),
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextAlignment = UITextAlignment.Center,
                Lines = 0,
                TextColor = Theme.GetContrastBasedColor(Theme.LoginColor)
            };

            _dialogView.Add(_messageView); 

            _dialogView.AddConstraint(_hasTitle ? 
                NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, title, NSLayoutAttribute.Bottom, 1f, _globalMargin) : 
                NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Top, 1f, _dialogEdgeConstraint));
            
            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                    NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                });
           
            AddSubviews (_dialogView);
        }

        private void InitPrompt(Action cancelAction, bool isNumericOnly = false, string inputText = "")
        {
            _isPrompt = true;

            Frame = UIScreen.MainScreen.Bounds;

            BackgroundColor = UIColor.Black.ColorWithAlpha(0.35f);

            _dialogView = new UIView(new CGRect(0, 0, Frame.Width - (_dialogMargin * 2), 0))
                {
                    BackgroundColor = Theme.LoginColor,
                    Center = this.Center,
                    Alpha = 0,
                };

            _dialogView.Layer.CornerRadius = 10;

            var lblTitle = new UILabel()
                {
                    Text = _title,
                    Font = UIFont.BoldSystemFontOfSize(16),
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextAlignment = UITextAlignment.Center,
                    Lines = 0,
                    TextColor = Theme.GetContrastBasedColor(Theme.LoginColor)
                };
            
            _dialogView.Add(lblTitle); 
            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(lblTitle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Top, 1f, _dialogEdgeConstraint),
                    NSLayoutConstraint.Create(lblTitle, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                    NSLayoutConstraint.Create(lblTitle, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                });

            if(!string.IsNullOrEmpty(_message))
            {
                _messageView = new UILabel()
                    {
                        Text = _message,
                        Font = UIFont.SystemFontOfSize(16),
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextAlignment = UITextAlignment.Center,
                        Lines = 0,
                        TextColor = Theme.GetContrastBasedColor(Theme.LoginColor)
                    };

                _dialogView.Add(_messageView); 

                _dialogView.AddConstraints(new [] 
                    {
                        NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lblTitle, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                        NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                        NSLayoutConstraint.Create(_messageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                    }); 
            }

            CustomInputView = new UITextView()
            {
                    Text = inputText,
                    Font = UIFont.SystemFontOfSize(16),
                TranslatesAutoresizingMaskIntoConstraints = false,
                KeyboardType = isNumericOnly ? UIKeyboardType.NumberPad : UIKeyboardType.Default,
            };

            _dialogView.Add(CustomInputView); 

            _dialogView.AddConstraint(
                NSLayoutConstraint.Create(CustomInputView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, string.IsNullOrEmpty(_message) ? lblTitle : _messageView, NSLayoutAttribute.Bottom, 1f, _globalMargin));
            
            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(CustomInputView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _dialogEdgeConstraint),
                    NSLayoutConstraint.Create(CustomInputView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_dialogEdgeConstraint),
                    GetHeightConstraint(CustomInputView),
                }); 

            AddSubviews (_dialogView);

            InitDoubleButtons(Localize.GetValue("Cancel"), Localize.GetValue("OkButtonText"), () =>{}, cancelAction, CustomInputView);
        }

        private void InitCancelButton(string title)
        {
            var cancelButton = GetNormalButton(title, () => {}, true);

            _dialogView.Add(cancelButton); 

            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(cancelButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _messageView, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                    NSLayoutConstraint.Create(cancelButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _globalMargin),
                    NSLayoutConstraint.Create(cancelButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_globalMargin),
                    GetHeightConstraint(cancelButton),
                });
            
        }

        private void InitDoubleButtons(string negativeTitle, string positiveTitle, Action positiveAction, Action negativeAction, UIView topView)
        {
            var view = new UIView(){TranslatesAutoresizingMaskIntoConstraints = false};

            _dialogView.Add(view); 

            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Top, NSLayoutRelation.Equal, topView, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _globalMargin),
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_globalMargin),
                    GetHeightConstraint(view),
                });

            var negativeButton = GetNormalButton(negativeTitle, negativeAction, true);
            var positiveButton = GetPositiveButton(positiveTitle, positiveAction); 

            view.Add(negativeButton); 
            view.Add(positiveButton); 

            view.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 0),
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, 0f),
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 0f),
                });
            
            view.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 0),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, 0f),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 0f),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, negativeButton, NSLayoutAttribute.Width, 1f, 0f),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, negativeButton, NSLayoutAttribute.Trailing, 1f, 10f),
                });
        }

        private void InitTrippleButtons(string negativeTitle, string positiveTitle, string neutralTitle, Action negativeAction, Action positiveAction, Action thirdAction)
        {
            neutralTitle = string.IsNullOrEmpty(neutralTitle) ? "Cancel" : neutralTitle;

            var view = new UIView(){TranslatesAutoresizingMaskIntoConstraints = false};
            var viewHeight = (_buttonsHeight * 3) + (_globalMargin * 2);

            _dialogView.Add(view); 
            _dialogView.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _messageView, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Leading, 1f, _globalMargin),
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _dialogView, NSLayoutAttribute.Trailing, 1f, -_globalMargin),
                    NSLayoutConstraint.Create(view, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, viewHeight),
                });
            
            var negativeButton = GetNormalButton(negativeTitle, negativeAction, true);
            var neutralButton = GetNormalButton(neutralTitle, thirdAction);
            var positiveButton = GetPositiveButton(positiveTitle, positiveAction); 

            view.Add(negativeButton); 
            view.Add(neutralButton); 
            view.Add(positiveButton); 

            view.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 0f),
                    NSLayoutConstraint.Create(negativeButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 0f),
                    GetHeightConstraint(negativeButton),
                });
            
            view.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(neutralButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, negativeButton, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                    NSLayoutConstraint.Create(neutralButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 0f),
                    NSLayoutConstraint.Create(neutralButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 0f),
                    GetHeightConstraint(neutralButton),
                });
            
            view.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, neutralButton, NSLayoutAttribute.Bottom, 1f, _globalMargin),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 0f),
                    NSLayoutConstraint.Create(positiveButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 0),
                    GetHeightConstraint(positiveButton),
                });
            
        }

        private NSLayoutConstraint GetHeightConstraint(UIView view)
        {
            return NSLayoutConstraint.Create(view, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, _buttonsHeight);
        }

        private UIButton GetNormalButton(string title, Action action, bool isCancellation = false)
        {
            var button = new FlatButton()
                {
                    Font = UIFont.SystemFontOfSize(16),
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = _dialogView.BackgroundColor,
                };

            button.SetTitle(title, UIControlState.Normal);
            FlatButtonStyle.Default.ApplyTo(button);
            button.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            button.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));
            button.Layer.CornerRadius = 5;

            button.TouchUpInside += (sender, e) =>
                {
                    if(action != null)
                    {
                        action();
                    }
                    if(isCancellation)
                    {
                        DismissEvent();
                    }
                    else
                    {
                        ClickedEvent();
                    }
                };

            return button;
        }

        private UIButton GetPositiveButton(string title, Action action)
        {
            var button = new FlatButton()
                {
                    Font = UIFont.SystemFontOfSize(16),
                    TranslatesAutoresizingMaskIntoConstraints = false,
                };

            button.SetTitle(title, UIControlState.Normal);
            FlatButtonStyle.Default.ApplyTo(button);
            button.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            button.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));
            button.Layer.CornerRadius = 5;

            var imageColor = UIColor.White.ColorWithAlpha(0.35f);
            button.SetBackgroundImage(GetImage(imageColor), UIControlState.Normal);
            button.SetBackgroundImage(GetImage(imageColor), UIControlState.Selected);
            button.SetBackgroundImage(GetImage(imageColor), UIControlState.Highlighted);

            button.TouchUpInside += (sender, e) =>
                {
                    if(action != null)
                    {
                        action();
                    }
                    ClickedEvent();
                };
            return button;
        }


        private async void AnimateShow()
        {
            LayoutIfNeeded();
            if(_dialogView.Subviews.Length > 0)
            {
                if(_dialogView.Subviews.Length  == 1)
                {
                    _dialogHeight =  (float)_dialogView.Subviews[0].Frame.Bottom - (float)_dialogView.Subviews[0].Frame.Top; 
                }
                else
                {
                    _dialogHeight =  (float)_dialogView.Subviews[_dialogView.Subviews.Length  - 1].Frame.Bottom - (float)_dialogView.Subviews[0].Frame.Top; 
                }
            }

            _dialogHeight += _dialogEdgeConstraint * 2;

            var y = (UIScreen.MainScreen.Bounds.Height - _dialogHeight) / 2;

            if (_isPrompt)
            {
                y= 50f;
                CustomInputView.BecomeFirstResponder();
            }

            _dialogView.Frame = new CGRect(_dialogView.Frame.X, y, _dialogView.Frame.Width, _dialogHeight);
            LayoutIfNeeded();

            await UIView.AnimateAsync(0.3, () =>
                {
                    _dialogView.Alpha = 1;
                });
        }

        private void DismissEvent()
        {
            if (Dismissed != null) {
                Dismissed.Invoke(this, EventArgs.Empty);
            }
            Dismiss();
        }

        private void ClickedEvent()
        {
            if (Clicked != null) {
                Clicked.Invoke(this, EventArgs.Empty);
            }
            Dismiss();
        }

        private async void Dismiss()
        {
            await UIView.AnimateAsync(0.3, () =>
                {
                    _dialogView.Alpha = 0;
                });
            if (_modalWindow == UIApplication.SharedApplication.KeyWindow)
            {
                UIApplication.SharedApplication.Windows[0].MakeKeyWindow();
            }
            this.RemoveFromSuperview();
            _modalWindow.Hidden = true;
            LayoutIfNeeded();
        }

        public override void TouchesBegan(Foundation.NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            var touchLocation = touch.LocationInView(this);

            if (CanBeDismissed && !_dialogView.Frame.Contains(touchLocation))
            {
                Dismiss();
            }
            base.TouchesBegan(touches, evt);
        }

        UIImage GetImage(UIColor color)
        {
            var rect = new CGRect(0f, 0f, 1f, 1f);
            UIGraphics.BeginImageContext(rect.Size);

            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(rect);

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }
    }
}

