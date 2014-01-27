using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LoginView : BaseViewController<LoginViewModel>
    {
        private bool _themeApplied;

		public LoginView () : base("LoginView", null)
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            if (!_themeApplied)
            {
                // set the theme of the company for the navigation bar
                ChangeThemeOfNavigationBar();
                _themeApplied = true;
            }

			NavigationController.NavigationBar.Hidden = true;
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			View.BackgroundColor = Theme.BackgroundColor;
			btnForgotPassword.SetTitleColor(Theme.LabelTextColor, UIControlState.Normal);

			txtEmail.Placeholder = Localize.GetValue("LoginViewEmailPlaceHolder");
			txtEmail.ReturnKeyType = UIReturnKeyType.Done;
			txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
			txtEmail.ShouldReturn = delegate {                          
				txtEmail.ResignFirstResponder ();
				return true;
			};

			txtPassword.Placeholder = Localize.GetValue("LoginViewPasswordPlaceHolder");
			txtPassword.SecureTextEntry = true;
			txtPassword.ReturnKeyType = UIReturnKeyType.Done;
			txtPassword.ShouldReturn = delegate {                          
				txtPassword.ResignFirstResponder ();
				return true;
			};  

			FlatButtonStyle.Clear.ApplyTo (btnForgotPassword);
			FlatButtonStyle.Main.ApplyTo (btnSignIn);

			btnSignIn.SetTitle (Localize.GetValue ("SignIn"), UIControlState.Normal);
			btnSignUp.SetTitle (Localize.GetValue ("Register"), UIControlState.Normal);

            var settings = this.Services().AppSettings;;

            var set = this.CreateBindingSet<LoginView, LoginViewModel>();

			if (settings.FacebookEnabled)
			{
				btnFbLogin.SetLeftImage("facebook_icon.png");
				btnFbLogin.SetTitle (Localize.GetValue ("Facebook"), UIControlState.Normal);
                set.Bind(btnFbLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginFacebook);
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;

            if (settings.TwitterEnabled)
			{
				btnTwLogin.SetLeftImage("twitter_icon.png");
				btnTwLogin.SetTitle (Localize.GetValue ("Twitter"), UIControlState.Normal);
                set.Bind(btnTwLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginTwitter);
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

			btnServer.SetTitle (Localize.GetValue ("ChangeServer"), UIControlState.Normal);
            btnServer.TouchUpInside += ChangeServerTouchUpInside;
			btnServer.Hidden = !settings.CanChangeServiceUrl;

            set.Bind(btnSignIn)
                .For("TouchUpInside")
				.To(vm => vm.SignInCommand);

            set.Bind(btnForgotPassword)
                .For("TouchUpInside")
                .To(vm => vm.ResetPassword);

            set.Bind(btnSignUp)
                .For("TouchUpInside")
                .To(vm => vm.SignUp);

            set.Bind(txtEmail)
                .For(v => v.Text)
                .To(vm => vm.Email);

            set.Bind(txtPassword)
                .For(v => v.Text)
                .To(vm => vm.Password);

            set.Apply();
        }

        void ChangeServerTouchUpInside (object sender, EventArgs e)
        {
            var popup = new UIAlertView {AlertViewStyle = UIAlertViewStyle.PlainTextInput, Title = "Server Url"};
            popup.GetTextField (0).Text = TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl;

            var cancelBtnIndex = popup.AddButton ("Cancel");
            var saveBtnIndex = popup.AddButton ("Save");

            popup.CancelButtonIndex = cancelBtnIndex;

            popup.Clicked += delegate(object sender2, UIButtonEventArgs e2) {
                if (e2.ButtonIndex == saveBtnIndex) {
                    TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl = popup.GetTextField (0).Text;                 
                } else {
                    popup.Dispose ();
                }
            };
            popup.Show ();
        }
    }
}

