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
using ClipboardManager = Android.Text.ClipboardManager;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "@string/LoginActivityName",
        Theme = "@style/LoginTheme",
		ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "taxihail",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
	public class LoginActivity : BaseBindingActivity<LoginViewModel>
    {
		private readonly FacebookService _facebookService;
		public LoginActivity ()
		{
			_facebookService = (FacebookService)TinyIoCContainer.Current.Resolve<IFacebookService>();
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			_facebookService.ActivityOnActivityResult(requestCode, resultCode, data);
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

            InitializeSoftKeyboardNavigation();
        }

	    private void InitializeSoftKeyboardNavigation()
	    {
            var userNameTextView = FindViewById<EditText>(Resource.Id.Username);
            var passwordTextView = FindViewById<EditText>(Resource.Id.Password);
            
            userNameTextView.NextFocusDownId = Resource.Id.Password;
            passwordTextView.NextFocusUpId = Resource.Id.Username;
            passwordTextView.NextFocusDownId = Resource.Id.SignInButton;

            passwordTextView.EditorAction += (sender, e) =>
            {
                e.Handled = false;
                if (e.ActionId == ImeAction.Done)
                {
                    ViewModel.SignInCommand.ExecuteIfPossible();
                    e.Handled = true;
                }
            };

            passwordTextView.SetTypeface(Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

            if (this.Services().Localize.IsRightToLeft)
            {
                passwordTextView.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
            }
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
            GC.Collect();
        }
	}
}