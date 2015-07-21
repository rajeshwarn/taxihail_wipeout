using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using Xamarin.FacebookBinding;
using ClipboardManager = Android.Text.ClipboardManager;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "Login",
        Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "taxihail",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
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
			base.OnViewModelSet ();

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
                FindViewById<Button>(Resource.Id.ServerButton).Click += (sender, e) => PromptServer();
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

			if(this.Services().Localize.IsRightToLeft)
			{
				password.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
			}

            ApplyKeyboardEnabler(username);
            ApplyKeyboardEnabler(password);

			ViewModel.SignInCommand.CanExecuteChanged += (sender, e) => {
				//just for the first one, it's a nudge to highlight the button as the next step
				if(ViewModel.SignInCommand.CanExecute(null))
				{
					FindViewById<Button>(Resource.Id.SignInButton).SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.button_main_selector));
				}
			};
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

        private async void PromptServer()
        {
            try
            {
                var serviceUrl = await this.Services().Message.ShowPromptDialog("Server Configuration",
                    "Enter Server Url",
                    null,
                    false,
                    this.Services().Settings.ServiceUrl
                );

                if(serviceUrl != null)
                {
                    ViewModel.SetServerUrl(serviceUrl);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
			_uiHelper.OnDestroy();           
            GC.Collect();
        }
	}
}