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
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Content.Res;

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

            DrawHelper.SupportLoginTextColor(FindViewById<TextView>(Resource.Id.ForgotPasswordButton));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.SignUpButton));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.SignInButton));

            if (!this.Services().Settings.FacebookEnabled)
			{
                FindViewById<Button>(Resource.Id.FacebookButton).Visibility = ViewStates.Invisible;
			}
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.FacebookButton));

            if (this.Services().Settings.CanChangeServiceUrl)
            {
                FindViewById<Button>(Resource.Id.ServerButton).Click += delegate { PromptServer(); };
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Visible;
                DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.ServerButton));
            }

            if (!this.Services().Settings.TwitterEnabled)
            {
                FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Invisible;
            }
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.TwitterButton));

            Observable.FromEventPattern<EventHandler, EventArgs>(
                ev => ViewModel.LoginSucceeded += ev,
                ev => ViewModel.LoginSucceeded -= ev)
                .Subscribe(_ => Observable.Timer(TimeSpan.FromSeconds(2))
                    .Subscribe(__ => RunOnUiThread(Finish)));

            var username = FindViewById<EditText>(Resource.Id.Username);
            var password = FindViewById<EditText>(Resource.Id.Password);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

            ApplyKeyboardEnabler(username);
            ApplyKeyboardEnabler(password);
        }

        public bool ShouldUseClipboardManager()
        {
            return PlatformHelper.IsAndroid23;
        }

        public void ApplyKeyboardEnabler(EditText view)
        {
            InputMethodManager mImm = (InputMethodManager)GetSystemService(Context.InputMethodService);  

            view.Click += (sender, e) =>
            {
                if (ShouldUseClipboardManager())
                {

                    ClipboardManager cm = (ClipboardManager)GetSystemService(Context.ClipboardService);
                    cm.Text = ((EditText)sender).Text;
                }

                mImm.ShowSoftInput(((EditText)sender), Android.Views.InputMethods.ShowFlags.Implicit);  
            };
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