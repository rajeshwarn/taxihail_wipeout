using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LoginView : MvxBindingTouchViewController<LoginViewModel>, INavigationView
    {

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code
        public LoginView () 
            : base(new MvxShowViewModelRequest<LoginViewModel>( null, true, new MvxRequestedBy()   ) )
        {
            
        }
        
        public LoginView (MvxShowViewModelRequest request) 
            : base(request)
        {
            
        }
        
        public LoginView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
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
            
			if (settings.FacebookEnabled)
			{
				btnFbLogin.SetLeftImage("Assets/Social/FB/fbIcon.png");
				btnFbLogin.SetTitle (Localize.GetValue ("FacebookButton"), UIControlState.Normal);               
                this.AddBindings (btnFbLogin, "{'TouchUpInside':{'Path':'LoginFacebook'}}");
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;

            if (settings.TwitterEnabled)
			{
				btnTwLogin.SetLeftImage("Assets/Social/TW/twIcon.png");
				btnTwLogin.SetTitle (Localize.GetValue ("TwitterButton"), UIControlState.Normal);
                this.AddBindings (btnTwLogin, "{'TouchUpInside':{'Path':'LoginTwitter'}}");
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

			btnServer.SetLeftImage("Assets/server.png");
			btnServer.SetTitle (Localize.GetValue ("Change Server"), UIControlState.Normal);
            btnServer.TouchUpInside += ChangeServerTouchUpInside;
			btnServer.Hidden = true;

            this.AddBindings (new Dictionary<object, string> {
                { btnSignIn, "{'TouchUpInside':{'Path':'SignInCommand'}}"}, 
				{ btnForgotPassword, "{'TouchUpInside':{'Path':'ResetPassword'}}"}, 
                { btnSignUp, "{'TouchUpInside':{'Path':'SignUp'}}"},               
                { txtEmail, "{'Text':{'Path':'Email'}}"},
                { txtPassword, "{'Text':{'Path':'Password'}}"},
            });
		
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

