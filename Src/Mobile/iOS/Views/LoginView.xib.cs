
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
using SocialNetworks.Services;
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

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class LoginView : MvxBindingTouchViewController<LoginViewModel>
    {

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code
		public LoginView() 
			: base(new MvxShowViewModelRequest<LoginViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
			
		}
		
		public LoginView(MvxShowViewModelRequest request) 
			: base(request)
		{
			
		}
		
		public LoginView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
			
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = true;
		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated); 

            TinyIoCContainer.Current.Resolve<IFacebookService>().ConnectionStatusChanged -= HandleFbConnectionStatusChanged;
            TinyIoCContainer.Current.Resolve<IFacebookService>().ConnectionStatusChanged += HandleFbConnectionStatusChanged;
             
            TinyIoCContainer.Current.Resolve<ITwitterService>().ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;
            TinyIoCContainer.Current.Resolve<ITwitterService>().ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;
             
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
             
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background_full.png"));
            
            var btnSignIn = AppButtons.CreateStandardButton(new RectangleF(25, 179, 120, 37), Resources.SignInButton, AppStyle.ButtonColor.Black);
            View.AddSubview(btnSignIn);

            var btnSignUp = AppButtons.CreateStandardButton(new RectangleF(175, 179, 120, 37), Resources.SignUpButton, AppStyle.ButtonColor.Black);
            View.AddSubview(btnSignUp);

            var btnPassword = AppButtons.CreateStandardButton(new RectangleF(170, 122, 140, 37), Resources.LoginForgotPasswordButton, AppStyle.ButtonColor.CorporateColor );
            View.AddSubview(btnPassword);       


            ((TextField)txtEmail).PaddingLeft = 5;
            ((TextField)txtEmail).StrokeColor = UIColor.FromRGBA(7, 34, 57, 255);

            txtEmail.Placeholder = Resources.EmailLabel;
            txtEmail.ReturnKeyType = UIReturnKeyType.Done;
            txtEmail.AutocapitalizationType = UITextAutocapitalizationType.None;
            txtEmail.AutocorrectionType = UITextAutocorrectionType.No;
            txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
            txtEmail.ShouldReturn = delegate
            {                          
                txtEmail.ResignFirstResponder();
                return true;
            };

            ((TextField)txtPassword).PaddingLeft = 5;
            ((TextField)txtPassword).StrokeColor = UIColor.FromRGBA(7, 34, 57, 255);

            txtPassword.Placeholder = Resources.PasswordLabel;
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.ShouldReturn = delegate
            {                          
                txtPassword.ResignFirstResponder();
                return true;
            };            

            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            if (settings.FacebookEnabled)
            {
				var btnFbLogin = AppButtons.CreateStandardButton(new RectangleF(55, 281, 211, 41), Resources.FacebookButton, AppStyle.ButtonColor.AlternateCorporateColor, "Assets/Social/FB/fbIcon.png");
                View.AddSubview(btnFbLogin);
                btnFbLogin.TouchUpInside += FacebookLogin;  
            }

            if (settings.TwitterEnabled)
            {
				var btnTwLogin = AppButtons.CreateStandardButton(new RectangleF(55, 342, 211, 41), Resources.TwitterButton, AppStyle.ButtonColor.AlternateCorporateColor, "Assets/Social/TW/twIcon.png" );
                View.AddSubview(btnTwLogin);
                btnTwLogin.TouchUpInside += TwitterLogin;   
            }

            if (settings.CanChangeServiceUrl)
            {
                var btnServer = AppButtons.CreateStandardButton(new RectangleF(55, 403, 211, 41), "Change Server", AppStyle.ButtonColor.AlternateCorporateColor, "Assets/server.png");
                btnServer.TouchUpInside += ChangeServerTouchUpInside;
                View.AddSubview(btnServer);            
            }

			this.AddBindings(new Dictionary<object, string>() {
				{ btnSignIn, "{'TouchUpInside':{'Path':'SignInCommand'}}"},	
                { btnPassword, "{'TouchUpInside':{'Path':'ResetPassword'}}"}, 
                { btnSignUp, "{'TouchUpInside':{'Path':'Signup'}}"}, 
				{ txtEmail, "{'Text':{'Path':'Email'}}"},
				{ txtPassword, "{'Text':{'Path':'Password'}}"},
			});
        }

        void ChangeServerTouchUpInside(object sender, EventArgs e)
        {
            var popup = new UIAlertView(){AlertViewStyle = UIAlertViewStyle.PlainTextInput};
            popup.Title = "Server Url";
            popup.GetTextField(0).Text = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl;
            var saveBtnIndex = popup.AddButton("Save");
            var cancelBtnIndex = popup.AddButton("Cancel");

            popup.CancelButtonIndex = cancelBtnIndex;

            popup.Clicked += delegate(object sender2, UIButtonEventArgs e2)
            {
                if (e2.ButtonIndex == saveBtnIndex)
                {
                    TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = popup.GetTextField(0).Text ;                 
                }
                else
                {
                    popup.Dispose();
                }
            };
            popup.Show();
        }

        private void LoadBackgroundNavBar(UINavigationBar bar)
        {
            bar.TintColor =  AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try
            {
                bar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch{ }
        }       
        #endregion

        private void FacebookLogin(object sender, EventArgs e)
        {
            if (TinyIoCContainer.Current.Resolve<IFacebookService>().IsConnected)
            {
                DoFbLogin();
            }
            else
            {
                TinyIoCContainer.Current.Resolve<IFacebookService>().Connect("email, publish_stream, publish_actions");

            }
        }

        void HandleFbConnectionStatusChanged(object sender, SocialNetworks.Services.Entities.FacebookStatus e)
        {
            if (e.IsConnected)
            {
                DoFbLogin();
            }
        }

        private void DoFbLogin()
        {
            LoadingOverlay.StartAnimatingLoading(LoadingOverlayPosition.Center, null, 130, 30);

            TinyIoCContainer.Current.Resolve<IFacebookService>().GetUserInfos(info => {
                var data = new RegisterAccount();
                data.FacebookId = info.Id;
                data.Email = info.Email;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

                try
                {
                    ThreadHelper.ExecuteInThread(() =>
                    {
                        try
                        {
                            InvokeOnMainThread(() => this.View.UserInteractionEnabled = false);
                            var service = TinyIoCContainer.Current.Resolve<IAccountService>();

                            var account = service.GetFacebookAccount(data.FacebookId);
                            if (account == null)
                            {
                                InvokeOnMainThread(() => ViewModel.Signup.Execute(data));
                            }
                            
                        }
                        finally
                        {
                            InvokeOnMainThread(() => this.View.UserInteractionEnabled = true);
                            LoadingOverlay.StopAnimatingLoading();
                        }
                    }
                    );
                }
                finally
                {
                    
                } 
            }, () => Console.WriteLine("A") 
            );
        }

        private void TwitterLogin(object sender, EventArgs e)
        {
            if (TinyIoCContainer.Current.Resolve<ITwitterService>().IsConnected)
            {
                DoTwLogin();
            }
            else
            {
                TinyIoCContainer.Current.Resolve<ITwitterService>().Connect();

            }
        }

        private void DoTwLogin()
        {
            LoadingOverlay.StartAnimatingLoading(LoadingOverlayPosition.Center, null, 130, 30);

            TinyIoCContainer.Current.Resolve<ITwitterService>().GetUserInfos(info => {
                var data = new RegisterAccount();
                data.TwitterId = info.Id;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");
        
                try
                {
                   
                    ThreadHelper.ExecuteInThread(() =>
                    {
                        try
                        {
                            InvokeOnMainThread(() => this.View.UserInteractionEnabled = false);
                            var service = TinyIoCContainer.Current.Resolve<IAccountService>();

                            Account account = service.GetTwitterAccount(data.TwitterId);
                            if (account == null)
                            {								
                                InvokeOnMainThread(() => ViewModel.Signup.Execute(data));
                            }
                            
                        }
                        finally
                        {
                            InvokeOnMainThread(() => 
                                {
                                    this.View.UserInteractionEnabled = true;
                                    LoadingOverlay.StopAnimatingLoading();
                                });
                        }
                    }
                    );
                }
                finally
                {
                    
                }
            }
            );
        }

        void HandleTwitterConnectionStatusChanged(object sender, SocialNetworks.Services.Entities.TwitterStatus e)
        {
            if (e.IsConnected)
            {
                DoTwLogin();
            }
        }

    }
}

