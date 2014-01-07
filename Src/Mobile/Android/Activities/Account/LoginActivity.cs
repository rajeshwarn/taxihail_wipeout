#if SOCIAL_NETWORKS
using SocialNetworks.Services;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoDroid;

#endif
using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
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
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Common.Configuration;
using Android.Text;
using System.Reactive.Linq;
using Xamarin.FacebookBinding;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait  )]
	public class LoginActivity : BaseBindingActivity<LoginViewModel>
    {
		private readonly IMessageService _messageService;
		private readonly FacebookService _facebookService;
		private UiLifecycleHelper _uiHelper;
		public static LoginActivity TopInstance { get; private set;}

		public LoginActivity ()
		{
			TopInstance = this;
			this._messageService = TinyIoCContainer.Current.Resolve<IMessageService>();
			this._facebookService = (FacebookService)TinyIoCContainer.Current.Resolve<IFacebookService>();

		}

#if SOCIAL_NETWORKS
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
            base.OnActivityResult(requestCode, resultCode, data);
                       
            (ViewModel.FacebookService as FacebookServicesMD).AuthorizeCallback(requestCode, (int)resultCode, data);
        }
#endif
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			_uiHelper = new UiLifecycleHelper(this, _facebookService.StatusCallback);
			_uiHelper.OnCreate(bundle);

		}
		protected override void OnPause()
		{
			base.OnPause();
			_uiHelper.OnPause();
		}
		protected override void OnResume()
		{
			base.OnResume();
			_uiHelper.OnResume();
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			_uiHelper.OnSaveInstanceState(outState);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			_uiHelper.OnActivityResult(requestCode, (int)resultCode, data);
			/*var facebookService = TinyIoCContainer.Current.Resolve<IFacebookService>() as FacebookService;

			switch (resultCode) {
				case Result.Ok:
					var accessToken = data.GetStringExtra("AccessToken");
					facebookService.SaveAccessToken(accessToken);
					break;
				case Result.Canceled:
					string error = data.GetStringExtra("Exception");
					_messageService.ShowMessage("Failed to Log In", "Reason:" + error);
					break;
				default:
					break;
			}*/
		}

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignIn; }
        }

        protected override void OnViewModelSet()
        {            
            SetContentView(Resource.Layout.View_Login);            

			if (!TinyIoCContainer.Current.Resolve<IAppSettings>().FacebookEnabled)
			{
				FindViewById<Button>(Resource.Id.FacebookButton).Visibility = ViewStates.Invisible;
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
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Invisible;
            }

            if (!TinyIoCContainer.Current.Resolve<IAppSettings>().TwitterEnabled)
            {
                FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Invisible;
            }           

            var linkResetPassword = FindViewById<UnderlineTextView>( Resource.Id.ForgotPasswordButton );

            linkResetPassword.SetTextColor( StyleManager.Current.NavigationTitleColor.ConvertToColor() );

            Observable.FromEventPattern<EventHandler, EventArgs>(
                ev => ViewModel.LoginSucceeded += ev,
                ev => ViewModel.LoginSucceeded -= ev)
                    .Subscribe( _ => Observable.Timer(TimeSpan.FromSeconds(2))  
                            .Subscribe(__ => RunOnUiThread(Finish)));

#if DEBUG
            FindViewById<EditText>(Resource.Id.Username).Text = "john@taxihail.com";
            FindViewById<EditText>(Resource.Id.Password).Text = "password";            
#endif 

        }

        private void PromptServer()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

            var input = new EditText(this);
            
			input.InputType = InputTypes.TextFlagNoSuggestions;
            input.Text = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl;
            alert.SetView(input);

            alert.SetPositiveButton("Ok", (s, e) =>
                {
                    var serverUrl = input.Text;

                    ViewModel.SetServerUrl(serverUrl);
                    


                });

            alert.SetNegativeButton("Cancel", (s, e) =>
            {
                
            });

            alert.Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
			_uiHelper.OnDestroy();           
            GC.Collect();
        }
	}
}