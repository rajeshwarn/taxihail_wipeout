using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using SocialNetworks.Services;
//using SocialNetworks.Services.MonoDroid;
using SocialNetworks.Services;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoDroid;
using SocialNetworks.Services.OAuth;
using TinyIoC;
using apcurium.Framework.Extensions;
using Android.Text.Util;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Validation;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class LoginActivity : Activity
    {
        private IFacebookService facebook;
        ITwitterService twitterService;

        /// <summary>
        /// use for SSO when FB app is isntalled
        /// </summary>
        /// <param name='requestCode'>
        /// Request code.
        /// </param>
        /// <param name='resultCode'>
        /// Result code.
        /// </param>
        /// <param name='data'>
        /// Data.
        /// </param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            (facebook as FacebookServicesMD).AuthorizeCallback(requestCode, (int)resultCode, data);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            InitializeSocialNetwork();

            

            if (AppContext.Current.LastEmail.HasValue())
            {
                FindViewById<EditText>(Resource.Id.Username).Text = AppContext.Current.LastEmail;
            }

            FindViewById<Button>(Resource.Id.SignUpButton).Click += new EventHandler(SignUpButton_Click);

            FindViewById<Button>(Resource.Id.SignInButton).Click += new EventHandler(btnSignIn_Click);

            FindViewById<Button>(Resource.Id.ForgotPasswordButton).Click += new EventHandler(ForgotPassword_Click);

            FindViewById<Button>(Resource.Id.FacebookButton).Click += delegate { facebook.Connect("email,publish_stream"); };

            FindViewById<Button>(Resource.Id.TwitterButton).Click += delegate { twitterService.Connect(); };

            facebook.ConnectionStatusChanged += (s, e) =>
            {
                 if(e.IsConnected)
                 {
                     facebook.GetUserInfos(infos => CheckIfFacebookAccountExist(infos));
                 }
            };

            twitterService.ConnectionStatusChanged += (s, e) =>
            {
                if (e.IsConnected)
                {
                    twitterService.GetUserInfos(infos => CheckIfTwitterAccountExist(infos));
                }
            };     

        }

        private void CheckIfFacebookAccountExist(UserInfos infos)
        {

                    string err = "";
                    var account = TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetFacebookAccount(infos.Id, out err);
                    if (account != null)
                    {
                        AppContext.Current.UpdateLoggedInUser(account, false);
                        AppContext.Current.LastEmail = account.Email;
                        RunOnUiThread(() =>
                        {
                            Finish();
                            StartActivity(typeof(MainActivity));
                        });
                        return;
                    }
                    else
                    {
                        DoSignUpWithParameter(infos.Firstname, infos.Lastname, infos.Email, "", FacebookId: infos.Id);
                    }
        }

        private void CheckIfTwitterAccountExist(UserInfos infos)
        {

            string err = "";
            var account = TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetTwitterAccount(infos.Id, out err);
            if (account != null)
            {
                AppContext.Current.UpdateLoggedInUser(account, false);
                AppContext.Current.LastEmail = account.Email;
                RunOnUiThread(() =>
                {
                    Finish();
                    StartActivity(typeof(MainActivity));
                });
                return;
            }
            else
            {
                DoSignUpWithParameter(infos.Firstname, infos.Lastname, infos.Email, "", TwitterId: infos.Id);
            }
        }

        private void InitializeSocialNetwork()
        {
            OAuthConfig oauthConfig = new OAuthConfig
            {
                ConsumerKey = "CIi418tFp0c4DIj8tQKEw",
                Callback = "http://www.apcurium.com/oauth",
                ConsumerSecret = "wtHZHvigOaKaXjHQT3MjdKZ8aICOa6toNcJlbfWX54",
                RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
                AccessTokenUrl = "https://twitter.com/oauth/access_token",
                AuthorizeUrl = "https://twitter.com/oauth/authorize"
            };

            facebook = new FacebookServicesMD("431321630224094", this);
            twitterService = new TwitterServiceMonoDroid(oauthConfig, this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AppContext.Current.LastEmail.HasValue())
            {
                FindViewById<EditText>(Resource.Id.Username).Text = AppContext.Current.LastEmail;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }

        void SignUpButton_Click(object sender, EventArgs e)
        {
            DoSignUp();
        }
        void ForgotPassword_Click(object sender, EventArgs e)
        {
            PasswordRecovery();
        }

        private void PasswordRecovery()
        {
            StartActivity(typeof(PasswordRecoveryActivity));
        }
        void btnSignIn_Click(object sender, EventArgs e)
        {
            DoSignIn();
        }

        private void DoSignIn()
        {


            ThreadHelper.ExecuteInThread(this, () =>
            {

                EditText txtUserName = FindViewById<EditText>(Resource.Id.Username);
                EditText txtPassword = FindViewById<EditText>(Resource.Id.Password);

                bool isValid = EmailValidation.IsValid(txtUserName.Text );

                if (isValid)
                {

                    string err = "";                    
                    var account = TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetAccount(txtUserName.Text, txtPassword.Text, out err);
                    if (account != null)
                    {
                        AppContext.Current.UpdateLoggedInUser( account, false );
                        AppContext.Current.LastEmail = account.Email;                        
                        RunOnUiThread(() =>
                        {
                            Finish();
                            StartActivity(typeof(MainActivity));
                        });
                        return;
                    }
                }
                            
                RunOnUiThread(() => this.ShowAlert(Resource.String.InvalidLoginMessageTitle, Resource.String.InvalidLoginMessage));
                            

            }, true);
        }
        private void DoSignUp()
        {
            StartActivity(typeof(SignUpActivity));
        }

        private void DoSignUpWithParameter(string FirstName, string LastName, string Email, string Phone, string TwitterId = "", string FacebookId ="")
        {
            Intent intent = new Intent(this, typeof(SignUpActivity));
            Bundle b = new Bundle();
            b.PutString("firstName", FirstName);
            b.PutString("lastName", LastName);
            b.PutString("email", Email);
            b.PutString("phone", Phone);
            b.PutString("twitterId", TwitterId);
            b.PutString("facebookId", FacebookId);
            intent.PutExtras(b);
            StartActivity(intent);
        }
    }
}