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
			btnForgotPassword.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

			FlatButtonStyle.Clear.ApplyTo (btnSupport);
			btnSupport.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);


			btnSignIn.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

			btnForgotPassword.SetTitle (Localize.GetValue ("LoginForgotPassword"), UIControlState.Normal);
			btnSupport.SetTitle (Localize.GetValue ("LoginSupport"), UIControlState.Normal);

			btnSignIn.SetTitle (Localize.GetValue ("SignIn"), UIControlState.Normal);
			btnSignUp.SetTitle (Localize.GetValue ("Register"), UIControlState.Normal);
            btnSignUp.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

            var settings = this.Services().Settings;

            btnSignIn.TouchUpInside += (sender, e) => { 
                var firstResponder = View.FindFirstResponder();
                if(firstResponder != null)
                {
                    firstResponder.ResignFirstResponder();
                }
            };

            var set = this.CreateBindingSet<LoginView, LoginViewModel>();

            if (settings.FacebookEnabled)
			{
				btnFbLogin.SetLeftImage("facebook_icon.png");
				btnFbLogin.SetTitle (Localize.GetValue ("Facebook"), UIControlState.Normal);
                set.Bind(btnFbLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginFacebook);
                btnFbLogin.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;

            if (settings.TwitterEnabled)
			{
				btnTwLogin.SetLeftImage("twitter_icon.png");
				btnTwLogin.SetTitle (Localize.GetValue ("Twitter"), UIControlState.Normal);
                set.Bind(btnTwLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginTwitter);
                btnTwLogin.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

			btnServer.SetTitle (Localize.GetValue ("ChangeServer"), UIControlState.Normal);
            btnServer.TouchUpInside += ChangeServerTouchUpInside;
            btnServer.Hidden = !settings.CanChangeServiceUrl;
            btnServer.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

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

            set.Bind(txtEmail)
                .For(v => v.Text)
                .To(vm => vm.Email);

            set.Bind(txtPassword)
                .For(v => v.Text)
                .To(vm => vm.Password);

            set.Apply();
        }

        async void ChangeServerTouchUpInside (object sender, EventArgs e)
        {
            try
            {            
                var serviceUrl = await this.Services().Message.ShowPromptDialog(
                    "Server Url",
                    string.Empty,
                    () => { return; },
                    false,
                    this.Services().Settings.ServiceUrl);

                if(serviceUrl != null)
                {
                    ViewModel.SetServerUrl(serviceUrl);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

