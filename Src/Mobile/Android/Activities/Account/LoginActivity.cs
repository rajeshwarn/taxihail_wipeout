using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using SocialNetworks.Services;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoDroid;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Validation;
using Android.Graphics;
using Android.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.Text;
using System.Collections.Generic;
using TinyMessenger;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode=Android.Content.PM.LaunchMode.SingleInstance)]
    public class LoginActivity : BaseBindingActivity<LoginViewModel>
    {
        public static LoginActivity TopInstance{get;set;}

        private ProgressDialog _progressDialog;
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
            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            facebook.SetCurrentContext(this);
            (facebook as FacebookServicesMD).AuthorizeCallback(requestCode, (int)resultCode, data);
        }

		public LoginActivity ()
		{
			TopInstance = this;
		}

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignIn; }
        }

        protected override void OnViewModelSet()
        {            

            //FindViewById<TextView>(Resource.Id.ForgotPasswordButton).PaintFlags = (FindViewById<TextView>(Resource.Id.ForgotPasswordButton).PaintFlags | Android.Graphics.PaintFlags.UnderlineText);

            SetContentView(Resource.Layout.View_Login);            

            _progressDialog = new ProgressDialog(this);
                      

            if (!TinyIoCContainer.Current.Resolve<IAppSettings>().FacebookEnabled)
            {
				FindViewById<Button>(Resource.Id.FacebookButton).Visibility = ViewStates.Gone;
            }

            if (TinyIoCContainer.Current.Resolve<IAppSettings>().CanChangeServiceUrl)
            {
                FindViewById<Button>(Resource.Id.ServerButton).Click += delegate
                {
                    PromptServer();
                };
            }
            else
            {
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Gone;
            }

            if (!TinyIoCContainer.Current.Resolve<IAppSettings>().TwitterEnabled)
            {
				FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Gone;
            }           

#if DEBUG
            FindViewById<EditText>(Resource.Id.Username).Text = "john@taxihail.com";
            FindViewById<EditText>(Resource.Id.Password).Text = "password";            
#endif 
        }

        private void HideProgressDialog()
        {
            RunOnUiThread(() =>
            {
                if (_progressDialog != null)
                {
                    _progressDialog.Dismiss();
                    _progressDialog = null;
                }
            });
        }

        private void ShowProgressDialog()
        {
            RunOnUiThread(() =>
            {
				_progressDialog = ProgressDialog.Show(this, "", this.GetString(Resource.String.LoadingMessage), true, false);
                _progressDialog.Show();

            });
        }

        private void PromptServer()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

            var input = new EditText(this);
            
            input.Text = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl;
            alert.SetView(input);

            alert.SetPositiveButton("Ok", (s, e) =>
                {
                    var serverUrl = input.Text;                    
                    TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = serverUrl;
                });

            alert.SetNegativeButton("Cancel", (s, e) =>
            {
                
            });

            alert.Show();
        }

        private void CancelAction(object sender, EventArgs e)
        {
            _progressDialog.CancelEvent -= CancelAction;
            _progressDialog.Cancel();
        }

        protected override void OnResume()
        {
            base.OnResume();            
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();           
            GC.Collect();
        }

    }
}