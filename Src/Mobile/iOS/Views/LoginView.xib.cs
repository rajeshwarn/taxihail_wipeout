using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LoginView : BaseViewController<LoginViewModel>
    {
		public LoginView () : base("LoginView", null)
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            if (!Theme.IsApplied)
            {
                // set the theme of the company for the navigation bar
                ChangeThemeOfNavigationBar();
                Theme.IsApplied = true;
            }

            NavigationController.NavigationBar.BarStyle = Theme.ShouldHaveLightContent(this.View.BackgroundColor)
                ? UIBarStyle.Black
                : UIBarStyle.Default;
			NavigationController.NavigationBar.Hidden = true;
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = Theme.LoginColor;

			txtEmail.Placeholder = Localize.GetValue("LoginViewEmailPlaceHolder");
            txtEmail.AccessibilityLabel = txtEmail.Placeholder;
            txtEmail.ReturnKeyType = UIReturnKeyType.Next;
			txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
			txtEmail.ShouldReturn = delegate {                          
				txtEmail.ResignFirstResponder ();
                txtPassword.BecomeFirstResponder();
				return true;
			};

			txtPassword.Placeholder = Localize.GetValue("LoginViewPasswordPlaceHolder");
            txtPassword.AccessibilityLabel = txtPassword.Placeholder;
			txtPassword.SecureTextEntry = true;
			txtPassword.ReturnKeyType = UIReturnKeyType.Done;
			txtPassword.ShouldReturn = delegate {                          
                txtPassword.ResignFirstResponder ();
				return true;
			};  

			FlatButtonStyle.Clear.ApplyTo (btnForgotPassword);
            btnForgotPassword.SetTitle (Localize.GetValue ("LoginForgotPassword"), UIControlState.Normal);
			btnForgotPassword.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);

			FlatButtonStyle.Clear.ApplyTo (btnSupport);
            btnSupport.SetTitle (Localize.GetValue ("LoginSupport"), UIControlState.Normal);
            btnSupport.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);

            btnSignIn.SetTitle (Localize.GetValue ("SignIn"), UIControlState.Normal);
			btnSignIn.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnSignIn.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));
            btnSignIn.TouchUpInside += (sender, e) => { 
                var firstResponder = View.FindFirstResponder();
                if(firstResponder != null)
                {
                    firstResponder.ResignFirstResponder();
                }
            };

			btnSignUp.SetTitle (Localize.GetValue ("Register"), UIControlState.Normal);
            btnSignUp.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnSignUp.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

            btnFbLogin.SetLeftImage("facebook_icon.png");
            btnFbLogin.SetTitle (Localize.GetValue ("Facebook"), UIControlState.Normal);
            btnFbLogin.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnFbLogin.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

            btnTwLogin.SetLeftImage("twitter_icon.png");
            btnTwLogin.SetTitle (Localize.GetValue ("Twitter"), UIControlState.Normal);
            btnTwLogin.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnTwLogin.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

            btnServer.SetTitle (Localize.GetValue ("ChangeServer"), UIControlState.Normal);
            btnServer.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnServer.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

            var set = this.CreateBindingSet<LoginView, LoginViewModel>();

            set.Bind(btnFbLogin)
                .For("TouchUpInside")
                .To(vm => vm.LoginFacebook);
            set.Bind(btnFbLogin)
                .For(v => v.Hidden)
                .To(vm => vm.Settings.FacebookEnabled)
                .WithConversion("BoolInverter");

            set.Bind(btnTwLogin)
                .For("TouchUpInside")
                .To(vm => vm.LoginTwitter);
            set.Bind(btnTwLogin)
                .For(v => v.Hidden)
                .To(vm => vm.Settings.TwitterEnabled)
                .WithConversion("BoolInverter");

            set.Bind(btnSignIn)
                .For("TouchUpInside")
				.To(vm => vm.SignInCommand);

            set.Bind(btnForgotPassword)
                .For("TouchUpInside")
                .To(vm => vm.ResetPassword);

			set.Bind(btnSupport)
				.For("TouchUpInside")
				.To(vm => vm.Support);
            set.Bind(btnSupport)
                .For(v => v.Hidden)
                .To(vm => vm.DisplayReportProblem)
                .WithConversion("BoolInverter");

            set.Bind(btnSignUp)
                .For("TouchUpInside")
                .To(vm => vm.SignUp);

            set.Bind(btnServer)
                .For("TouchUpInside")
                .To(vm => vm.PromptChangeServerUrl);
            set.Bind(btnServer)
                .For(v => v.Hidden)
                .To(vm => vm.Settings.CanChangeServiceUrl)
                .WithConversion("BoolInverter");

            set.Bind(txtEmail)
                .For(v => v.Text)
                .To(vm => vm.Email);

            set.Bind(txtPassword)
                .For(v => v.Text)
                .To(vm => vm.Password);

            set.Apply();
        }
    }
}

