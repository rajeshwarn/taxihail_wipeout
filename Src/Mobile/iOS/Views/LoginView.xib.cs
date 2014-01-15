using System;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LoginView : BaseViewController<LoginViewModel>, INavigationView
    {

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code
        public LoginView ()
            : base("LoginView", null)
        {
            
        }


        #region INavigationView implementation

        public bool HideNavigationBar {
            get { return true;}
        }

        #endregion


        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();


			FlatButtonStyle.Clear.ApplyTo(btnForgotPassword);
			FlatButtonStyle.Main.ApplyTo (btnSignIn);

			Localize.GetValue ("SignInButton");
			Localize.GetValue ("SignUpButton"); 

            txtEmail.Placeholder = Localize.GetValue("EmailLabel");
            txtEmail.ReturnKeyType = UIReturnKeyType.Done;

            txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
            txtEmail.ShouldReturn = delegate {                          
                txtEmail.ResignFirstResponder ();
                return true;
            };

            txtPassword.Placeholder = Localize.GetValue("PasswordLabel");
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.ShouldReturn = delegate {                          
                txtPassword.ResignFirstResponder ();
                return true;
            };            

            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();

            var set = this.CreateBindingSet<LoginView, LoginViewModel>();

			if (settings.FacebookEnabled)
			{
				btnFbLogin.SetLeftImage("Assets/Social/FB/facebook-icon.png");
				btnFbLogin.SetTitle (Localize.GetValue ("FacebookButton"), UIControlState.Normal);
                set.Bind(btnFbLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginFacebook);
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;

            if (settings.TwitterEnabled)
			{
				btnTwLogin.SetLeftImage("Assets/Social/TW/twitter.png");
				btnTwLogin.SetTitle (Localize.GetValue ("TwitterButton"), UIControlState.Normal);
                set.Bind(btnTwLogin)
                    .For("TouchUpInside")
                    .To(vm => vm.LoginTwitter);
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

			btnServer.SetLeftImage("Assets/server.png");
			btnServer.SetTitle (Localize.GetValue ("Change Server"), UIControlState.Normal);
            btnServer.TouchUpInside += ChangeServerTouchUpInside;
			btnServer.Hidden = true;

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

		
            ViewModel.Load ();
            View.ApplyAppFont();           
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

        #endregion
    }
}

