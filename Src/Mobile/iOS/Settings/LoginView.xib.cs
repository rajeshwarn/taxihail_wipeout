
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

namespace apcurium.MK.Booking.Mobile.Client
{
    public class txtDel : UITextFieldDelegate
    {
        
        
    }

    public partial class LoginView : UIViewController
    {


        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public LoginView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public LoginView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public LoginView() : base("LoginView", null)
        {
            Initialize();
        }

        void Initialize()
        {
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);            
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
            
            if (AppContext.Current.LastEmailUsed.HasValue())
            {
                txtEmail.Text = AppContext.Current.LastEmailUsed;
            }

            var btnSignIn = AppButtons.CreateStandardGradientButton(new RectangleF(25, 179, 120, 37), Resources.SignInButton, UIColor.White, AppStyle.ButtonColor.Black);
            View.AddSubview(btnSignIn);
            btnSignIn.TouchUpInside += SignInClicked;

            var btnSignUp = AppButtons.CreateStandardGradientButton(new RectangleF(175, 179, 120, 37), Resources.SignUpButton, UIColor.White, AppStyle.ButtonColor.Black);
            View.AddSubview(btnSignUp);
            btnSignUp.TouchUpInside += SignUpClicked;

            var btnPassword = AppButtons.CreateStandardButton(new RectangleF(170, 122, 140, 37), Resources.LoginForgotPasswordButton, AppStyle.LightBlue, AppStyle.ButtonColor.Blue);
            View.AddSubview(btnPassword);
            btnPassword.TouchUpInside += PasswordTouchUpInside;         


            ((TextField)txtEmail).PaddingLeft = 5;
            ((TextField)txtEmail).StrokeColor = UIColor.FromRGBA(7, 34, 57, 255);

            txtEmail.Placeholder = Resources.EmailLabel;
            txtEmail.ReturnKeyType = UIReturnKeyType.Done;
            txtEmail.AutocapitalizationType = UITextAutocapitalizationType.None;
            txtEmail.AutocorrectionType = UITextAutocorrectionType.No;
            txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
            txtEmail.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };

            ((TextField)txtPassword).PaddingLeft = 5;
            ((TextField)txtPassword).StrokeColor = UIColor.FromRGBA(7, 34, 57, 255);

            txtPassword.Placeholder = Resources.PasswordLabel;
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };
            
            txtEmail.EditingDidEnd += delegate
            {
                txtEmail.Text = StringHelper.RemoveDiacritics(txtEmail.Text).ToLower();
            };
            


            var btnFbLogin = AppButtons.CreateStandardImageButton(new RectangleF(55, 281, 211, 41), Resources.FacebookLoginBtn, AppStyle.LightBlue, "Assets/Social/FB/fbIcon.png", AppStyle.ButtonColor.DarkBlue);
            View.AddSubview(btnFbLogin);
            btnFbLogin.TouchUpInside += FacebookLogin;  

            var btnTwLogin = AppButtons.CreateStandardImageButton(new RectangleF(55, 342, 211, 41), Resources.TwitterLoginBtn, AppStyle.LightBlue, "Assets/Social/TW/twIcon.png", AppStyle.ButtonColor.DarkBlue);
            View.AddSubview(btnTwLogin);
            btnTwLogin.TouchUpInside += TwitterLogin;   

            

            //txtEmail.BecomeFirstResponder();

        }
         
        void SignUpClicked(object sender, EventArgs e)
        {
            ShowSignUp(null);
        }

        private void ShowSignUp(RegisterAccount accountData)
        {
            CreateAccountView view;
            if (accountData != null)
            {
                view = new CreateAccountView(accountData);
            }
            else
            {
                view = new CreateAccountView();
            }

            view.AccountCreated += delegate(object data, EventArgs e2)
            {

                if (data is RegisterAccount)
                {

                    if (((RegisterAccount)data).FacebookId.HasValue() || ((RegisterAccount)data).TwitterId.HasValue())
                    {
                        var facebookId = ((RegisterAccount)data).FacebookId;
                        var twitterId = ((RegisterAccount)data).TwitterId;
                        LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
                        ThreadHelper.ExecuteInThread(() =>
                        {
                            try
                            {
                                Thread.Sleep(500);                             
                                Account account = null;
                                string error = "";
                                var service = TinyIoCContainer.Current.Resolve<IAccountService>();
                                if ( facebookId.HasValue() )
                                {

                                    account = service.GetFacebookAccount(facebookId, out error);
                                }
                                else
                                {
                                    account = service.GetTwitterAccount(twitterId, out error);
                                }
                                if (account != null)
                                {
                                    SetAccountInfo(account);
                                }
                            }
                            catch
                            {
                            }
                            finally
                            {                 
                                LoadingOverlay.StopAnimatingLoading(this.View);
                            }
                        });
                    }
                    else
                    {
                        InvokeOnMainThread(() => txtEmail.Text = ((RegisterAccount)data).Email);
                    }
                }
            };
            
            var nav = new UINavigationController(view);
            //nav.NavigationBar.TintColor = UIColor.FromRGB(255, 178, 14);
            LoadBackgroundNavBar(nav.NavigationBar);
            nav.Title = ".";
            this.PresentModalViewController(nav, true);
        }

        private void LoadBackgroundNavBar(UINavigationBar bar)
        {
            bar.TintColor = UIColor.FromRGB(0, 78, 145);

            //It might crash on iOS version smaller than 5.0
            try
            {
                bar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch
            {
            }
        }

        void PasswordTouchUpInside(object sender, EventArgs e)
        {
            var nav = new UINavigationController(new ResetPasswordView());
            //nav.NavigationBar.TintColor = UIColor.FromRGB(255, 178, 14);
            LoadBackgroundNavBar(nav.NavigationBar);
            nav.Title = ".";
            this.PresentModalViewController(nav, true);
        }

        private void SetAccountInfo(Account account)
        {
            AppContext.Current.LastEmailUsed = txtEmail.Text;
            AppContext.Current.LoggedInEmail = txtEmail.Text;
            InvokeOnMainThread(() => AppContext.Current.UpdateLoggedInUser(account, false));
            InvokeOnMainThread(() => this.DismissModalViewControllerAnimated(true));
            if (AppContext.Current.Controller.SelectedRefreshableViewController != null)
            {
                AppContext.Current.Controller.View.InvokeOnMainThread(() => 
                {
                    AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData();
                }
                );
            }
        }

        void SignInClicked(object sender, EventArgs e)
        {

            try
            {
                AppContext.Current.SignOutUser();
                LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
                ThreadHelper.ExecuteInThread(() =>
                {
                    try
                    {
                        
                        InvokeOnMainThread(() => this.View.UserInteractionEnabled = false);
                        var service = TinyIoCContainer.Current.Resolve<IAccountService>();
                        string error = "";                      
                        var account = service.GetAccount(txtEmail.Text, txtPassword.Text, out error);
                        if (account != null)
                        {
                            SetAccountInfo(account);
                        }
                        else
                        {
                            if (error.IsNullOrEmpty())
                            {
                                MessageHelper.Show(Resources.InvalidLoginMessageTitle, Resources.AccountNotValidatedMessage, Resources.ResendValidationButton, () => service.ResendConfirmationEmail(txtEmail.Text));
                            }
                            else
                            {
                                MessageHelper.Show(Resources.InvalidLoginMessageTitle, Resources.InvalidLoginMessage + " (" + error + ")");
                            }
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
            finally
            {
                
            }

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
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);

            TinyIoCContainer.Current.Resolve<IFacebookService>().GetUserInfos(info => {
                var data = new RegisterAccount();
                data.FacebookId = info.Id;
                data.Email = info.Email;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

                try
                {
                    AppContext.Current.SignOutUser();
                    ThreadHelper.ExecuteInThread(() =>
                    {
                        try
                        {
                            InvokeOnMainThread(() => this.View.UserInteractionEnabled = false);
                            var service = TinyIoCContainer.Current.Resolve<IAccountService>();


                            string error = "";

                            var account = service.GetFacebookAccount(data.FacebookId, out error);
                            if (account != null)
                            {
                                SetAccountInfo(account);
                            }
                            else
                            {
                                InvokeOnMainThread(() => ShowSignUp(data));
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
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);

            TinyIoCContainer.Current.Resolve<ITwitterService>().GetUserInfos(info => {
                var data = new RegisterAccount();
                data.TwitterId = info.Id;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");
        
                try
                {
                    AppContext.Current.SignOutUser();
                    ThreadHelper.ExecuteInThread(() =>
                    {
                        try
                        {
                            InvokeOnMainThread(() => this.View.UserInteractionEnabled = false);
                            var service = TinyIoCContainer.Current.Resolve<IAccountService>();

                            string error = "";
                            Account account = service.GetTwitterAccount( data.TwitterId, out error );
                            if (account != null)
                            {
                                SetAccountInfo( account );
                            }
                            else
                            {
                                InvokeOnMainThread(() => ShowSignUp(data));
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

