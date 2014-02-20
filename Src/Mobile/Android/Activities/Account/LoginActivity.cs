using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ViewModels;
using Xamarin.FacebookBinding;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "Login", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
	public class LoginActivity : BaseBindingActivity<LoginViewModel>
    {
		private readonly FacebookService _facebookService;
		private UiLifecycleHelper _uiHelper;
		public LoginActivity ()
		{
			_facebookService = (FacebookService)TinyIoCContainer.Current.Resolve<IFacebookService>();
		}

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
		}

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Login);

            var settings = this.Services().Settings;

            if (!settings.FacebookEnabled)
			{
                FindViewById<Button>(Resource.Id.FacebookButton).Visibility = ViewStates.Invisible;
			}

            if (settings.CanChangeServiceUrl)
            {
                FindViewById<Button>(Resource.Id.ServerButton).Click += delegate { PromptServer(); };
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Visible;
            }

            if (!settings.TwitterEnabled)
            {
                FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Invisible;
            }

            Observable.FromEventPattern<EventHandler, EventArgs>(
                ev => ViewModel.LoginSucceeded += ev,
                ev => ViewModel.LoginSucceeded -= ev)
                .Subscribe(_ => Observable.Timer(TimeSpan.FromSeconds(2))
                    .Subscribe(__ => RunOnUiThread(Finish)));

			EditText password = FindViewById<EditText>(Resource.Id.Password);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

            if (ShouldUseClipboardManager())
            {
                EditText username = FindViewById<EditText>(Resource.Id.Username);
                username.Click += (object sender, EventArgs e) => 
                {
                    ClipboardManager cm = (ClipboardManager) Application.Context.GetSystemService(Context.ClipboardService);
                    cm.Text = username.Text;
                };
            }
        }

        public bool ShouldUseClipboardManager()
        {
            return (int) Build.VERSION.SdkInt <= 8;
        }

        private void PromptServer()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

            var input = new EditText(this)
            {
                InputType = InputTypes.TextFlagNoSuggestions,
                Text = this.Services().Settings.ServiceUrl
            };

            alert.SetView(input);

            alert.SetPositiveButton("Ok", (s, e) =>
            {
                var serverUrl = input.Text;

                ViewModel.SetServerUrl(serverUrl);
            });

            alert.SetNegativeButton("Cancel", (s, e) => { });

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