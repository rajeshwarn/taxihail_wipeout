using System;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class ResetPasswordView : DialogViewController
    {
        private EntryElement _emailEntry;
        private string _email;

        public ResetPasswordView() : base(null)
        {
        }

        public override void ViewDidLoad()
        {
            _email = "";
            LoadSettingsElements();
            
            
            AddButton(170, 100, Resources.ResetPasswordReset, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor, () => ResetPassword(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);
            AddButton(20, 100, Resources.ResetPasswordCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor, () => Cancel(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Grey);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.BackgroundColor = UIColor.Clear; // UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
			((UINavigationController ) ParentViewController ).NavigationBar.TopItem.TitleView = new TitleView(null, Resources.View_PasswordRecovery,true);
            ((UINavigationController ) ParentViewController ).View.BackgroundColor =UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
        }
        private void ResetPassword()
        {
            _emailEntry.FetchValue();
            
            
            
            if (!IsEmail(_email))
            {
                MessageHelper.Show(Resources.ResetPasswordInvalidDataTitle, Resources.ResetPasswordInvalidDataMessage);
                return;
            }
            
            
            
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var result = TinyIoCContainer.Current.Resolve<IAccountService>().ResetPassword(_email);
                    if (result)
                    {
                        InvokeOnMainThread(() => MessageHelper.Show(Resources.ResetPasswordConfirmationTitle, Resources.ResetPasswordConfirmationMessage));
                        BeginInvokeOnMainThread(() => this.DismissModalViewControllerAnimated(true));
                    }
                    else
                    {
                        BeginInvokeOnMainThread(() => MessageHelper.Show(Resources.NoConnectionTitle, Resources.NoConnectionMessage));
                    }
                }
                finally
                {
                    InvokeOnMainThread(() => this.View.UserInteractionEnabled = true);
                    LoadingOverlay.StopAnimatingLoading(this.View);
                }
            }
            );
            
            
            
            
        }
        
//      public override string GetTitle ()
//      {
//          //return Resources.ResetPasswordTitle;
//          return "";
//      }
//      
        public static bool IsEmail(string inputEmail)
        {
            inputEmail = inputEmail.ToSafeString();
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }

        private void Cancel()
        {
            this.DismissModalViewControllerAnimated(true);
        }

        public override void ViewDidAppear(bool animated)
        {
            
        }

        public override void LoadView()
        {
            base.LoadView();
            
        }

//      public override UIColor GetCellColor ()
//      {
//          return UIColor.White;
//      }
        private void LoadSettingsElements()
        {
            
            
			var menu = new RootElement(Resources.View_PasswordRecovery);
            
			var settings = new Section(Resources.View_PasswordRecovery);
            
            

            _emailEntry = new RightAlignedEntryElement (Resources.ResetPasswordLabel, Resources.ResetPasswordPlaceholder, _email);
            _emailEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.EmailAddress;
            _emailEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
            _emailEntry.AutocorrectionType = UITextAutocorrectionType.No;

            _emailEntry.Changed += delegate
            {
                _emailEntry.FetchValue();
                _email = _emailEntry.Value;
            };
            
            menu.Add(settings);
            settings.Add(_emailEntry);
            
            
            
            this.InvokeOnMainThread(() => {
                this.Root = menu; });
            
            
        }

        private void LoadBackgroundNavBar(UINavigationBar bar)
        {
            bar.TintColor =  AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try
            {
                bar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch
            {
            }
        }

        private void AddButton(float x, float y, string title, UIColor normal, UIColor selected, Action clicked, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor);
            btn.TextShadowColor = null;
//          var btn = UIButton.FromType (UIButtonType.RoundedRect);
//          btn.Frame = new System.Drawing.RectangleF (x, y, 130, 40);
//          btn.SetTitle (title, UIControlState.Normal);
            this.View.AddSubview(btn);
            btn.TouchUpInside += delegate
            {
                clicked();
            };
            //GlassButton.Wrap (btn, normal, selected).TouchUpInside += delegate { clicked (); };
            
            //btn.TouchUpInside += delegate { clicked (); };
            
        }
        
        
        
    }
}

