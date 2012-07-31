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
using Microsoft.Practices.ServiceLocation;
using TaxiMobileApp;
using Android.Text.Util;

namespace TaxiMobile
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", NoHistory=true)]
    public class Login : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);
            
            FindViewById<TextView>(Resource.Id.SignUpLink).Click += new EventHandler(SignUpLink_Click);
            var btnSignIn = FindViewById<Button>(Resource.Id.SignInButton);
            btnSignIn.Click += new EventHandler(btnSignIn_Click);

        }

       
        void SignUpLink_Click(object sender, EventArgs e)
        {
            DoSignUp();
        }

        void btnSignIn_Click(object sender, EventArgs e)
        {
            DoSignIn();
        }

        void btnSignIn_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.E.Action == MotionEventActions.Up)
            {
                DoSignUp();

            }
        }

        private void DoSignUp()
        {

        }

        private void DoSignIn()
        {
            var dialog = ProgressDialog.Show(this, "", GetString(Resource.String.LoadingMessage), true, false);

            ThreadHelper.ExecuteInThread(() =>
            {

                string err = "";
                var account = ServiceLocator.Current.GetInstance<IAccountService>().GetAccount("alex.proteau@e-nergik.com", "wisdom", out err);


                RunOnUiThread(() =>
                {
                    if (account != null)
                    {
                        EditText txtUserName = FindViewById<EditText>(Resource.Id.Username);
                        //EditText txtPassword = FindViewById<EditText>(Resource.Id.Password);
                        txtUserName.Text = account.Name;
                        StartActivity(typeof(MainActivity));
                    }
                    dialog.Cancel();
                });


            });
        }
    }
}