using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using MonoTouch.MessageUI;
using System.IO;
using System.Drawing;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Navigation;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using TinyMessenger;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class LoginView : BaseViewController<LoginViewModel>, INavigationView
    {

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code
        public LoginView () 
            : base(new MvxShowViewModelRequest<LoginViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
        }

        #region INavigationView implementation

        public bool HideNavigationBar {
            get { return true;}
        }

        #endregion

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);          
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
             
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full.png"));                       

            AppButtons.FormatStandardButton (btnSignIn, Resources.SignInButton, AppStyle.ButtonColor.Black, null);
            AppButtons.FormatStandardButton (btnSignUp, Resources.SignUpButton, AppStyle.ButtonColor.Grey, null);          

            ((TextField)txtEmail).PaddingLeft = 5;
            ((TextField)txtEmail).StrokeColor = UIColor.FromRGBA (7, 34, 57, 255);
            ((TextField)txtEmail).FieldHeight = 48;
            ((TextField)txtPassword).FieldHeight = 48;

            txtEmail.Placeholder = Resources.EmailLabel;
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

            txtPassword.Placeholder = Resources.PasswordLabel;
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.ShouldReturn = delegate {                          
                txtPassword.ResignFirstResponder ();
                return true;
            };            


            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
            if (settings.FacebookEnabled)
			{
                AppButtons.FormatStandardButton (btnFbLogin, Resources.FacebookButton, AppStyle.ButtonColor.Grey, "Assets/Social/FB/fbIcon.png");               
                this.AddBindings (btnFbLogin, "{'TouchUpInside':{'Path':'LoginFacebook'}}");
            }
            btnFbLogin.Hidden = !settings.FacebookEnabled;


            if (settings.TwitterEnabled)
			{
                AppButtons.FormatStandardButton (btnTwLogin, Resources.TwitterButton, AppStyle.ButtonColor.Grey, "Assets/Social/TW/twIcon.png");
                this.AddBindings (btnTwLogin, "{'TouchUpInside':{'Path':'LoginTwitter'}}");
            }
            btnTwLogin.Hidden = !settings.TwitterEnabled;

            if (settings.CanChangeServiceUrl)
			{
                AppButtons.FormatStandardButton (btnServer, "Change Server", AppStyle.ButtonColor.Grey, "Assets/server.png");
                btnServer.TouchUpInside += ChangeServerTouchUpInside;
            }
            btnServer.Hidden = !settings.CanChangeServiceUrl;

            linkForgotPassword.TextColor = AppStyle.NavigationTitleColor;
            
            
            this.AddBindings (new Dictionary<object, string> () {
                { btnSignIn, "{'TouchUpInside':{'Path':'SignInCommand'}}"}, 
                { linkForgotPassword, "{'TouchUpInside':{'Path':'ResetPassword'}}"}, 
                { btnSignUp, "{'TouchUpInside':{'Path':'SignUp'}}"},               
                { txtEmail, "{'Text':{'Path':'Email'}}"},
                { txtPassword, "{'Text':{'Path':'Password'}}"},
            });


            if (!UIHelper.Is4InchDisplay)
            {

                btnSignUp.IncrementY(-25);
            }

            ViewModel.Load ();
            this.View.ApplyAppFont();           

        }

        void ChangeServerTouchUpInside (object sender, EventArgs e)
        {
            var popup = new UIAlertView (){AlertViewStyle = UIAlertViewStyle.PlainTextInput};
            popup.Title = "Server Url";
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
       
        private void LoadBackgroundNavBar (UINavigationBar bar)
        {
            bar.TintColor = AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try {
                bar.SetBackgroundImage (UIImage.FromFile ("Assets/navBar.png"), UIBarMetrics.Default);
            } catch {
            }
        }       

        #endregion
    }
}

