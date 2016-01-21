using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using ClipboardManager = Android.Text.ClipboardManager;

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
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.FacebookButton));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.TwitterButton));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.ServerButton));

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
                    if (ViewModel.SignInCommand.CanExecute())
                    {
                        ViewModel.SignInCommand.Execute();
                    }
                    e.Handled = true;
                }
            };

            passwordTextView.SetTypeface(Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

            if (this.Services().Localize.IsRightToLeft)
            {
                passwordTextView.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
            }
	    }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
	}
}