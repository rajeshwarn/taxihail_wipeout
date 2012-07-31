using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Validation;

namespace TaxiMobile.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait)]
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
                    var account = ServiceLocator.Current.GetInstance<IAccountService>().GetAccount(txtUserName.Text, txtPassword.Text, out err);
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