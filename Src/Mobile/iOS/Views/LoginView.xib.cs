using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LoginView : MvxViewController, INavigationView
    {
		public LoginView () 
			: base("LoginView", null)
        {

        }

		public new LoginViewModel ViewModel
		{
			get
			{
				return (LoginViewModel)DataContext;
			}
		}


        #region INavigationView implementation

        public bool HideNavigationBar {
            get { return true;}
        }

        #endregion


        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
             
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full.png"));

            AppButtons.FormatStandardButton(btnSignIn, Localize.GetValue("SignInButton"), AppStyle.ButtonColor.Black);
            AppButtons.FormatStandardButton(btnSignUp, Localize.GetValue("SignUpButton"), AppStyle.ButtonColor.Grey);          

            ((TextField)txtEmail).PaddingLeft = 5;
            ((TextField)txtEmail).StrokeColor = UIColor.FromRGBA (7, 34, 57, 255);
            ((TextField)txtEmail).FieldHeight = 48;
            ((TextField)txtPassword).FieldHeight = 48;

            txtEmail.Placeholder = Localize.GetValue("EmailLabel");
            txtEmail.ReturnKeyType = UIReturnKeyType.Done;
            txtEmail.AutocapitalizationType = UITextAutocapitalizationType.None;
            txtEmail.AutocorrectionType = UITextAutocorrectionType.No;
            txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
            txtEmail.ShouldReturn = delegate {                          
                txtEmail.ResignFirstResponder ();
                return true;
            };

            ((TextField)txtPassword).PaddingLeft = 5;
            ((TextField)txtPassword).StrokeColor = UIColor.FromRGBA (7, 34, 57, 255);

            txtPassword.Placeholder = Localize.GetValue("PasswordLabel");
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.ShouldReturn = delegate {                          
                txtPassword.ResignFirstResponder ();
                return true;
            };            


            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
            if (settings.FacebookEnabled)
			{
                AppButtons.FormatStandardButton(btnFbLogin, Localize.GetValue("FacebookButton"), AppStyle.ButtonColor.Grey, "Assets/Social/FB/fbIcon.png");               
				this.AddBindings(btnFbLogin, "TouchUpInside LoginFacebook");
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;


            if (settings.TwitterEnabled)
			{
                AppButtons.FormatStandardButton(btnTwLogin, Localize.GetValue("TwitterButton"), AppStyle.ButtonColor.Grey, "Assets/Social/TW/twIcon.png");
				this.AddBindings (btnTwLogin, "TouchUpInside LoginTwitter");
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

            if (settings.CanChangeServiceUrl)
			{
                AppButtons.FormatStandardButton (btnServer, "Change Server", AppStyle.ButtonColor.Grey, "Assets/server.png");
                btnServer.TouchUpInside += ChangeServerTouchUpInside;
            }
            btnServer.Hidden = !settings.CanChangeServiceUrl;

            linkForgotPassword.TextColor = AppStyle.NavigationTitleColor;
            
            
            this.AddBindings (new Dictionary<object, string> {
				{ btnSignIn, "TouchUpInside SignInCommand"}, 
				{ linkForgotPassword, "TouchUpInside ResetPassword"}, 
				{ btnSignUp, "TouchUpInside SignUp"},               
				{ txtEmail, "Text Email"},
				{ txtPassword, "Text Password"},
            });


            if (!UIHelper.Is4InchDisplay)
            {

                btnSignUp.IncrementY(-25);
            }

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
		
    }
}

