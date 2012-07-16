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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            if (AppContext.Current.LastEmail.HasValue())
            {
                FindViewById<EditText>(Resource.Id.Username).Text = AppContext.Current.LastEmail;
            }

            FindViewById<TextView>(Resource.Id.SignUpLink).Click += new EventHandler(SignUpLink_Click);

            FindViewById<Button>(Resource.Id.SignInButton).Click += new EventHandler(btnSignIn_Click);

            
            FindViewById<TextView>(Resource.Id.ForgotPasswordButton).Click += new EventHandler(ForgotPassword_Click);
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

        void SignUpLink_Click(object sender, EventArgs e)
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
    }
}