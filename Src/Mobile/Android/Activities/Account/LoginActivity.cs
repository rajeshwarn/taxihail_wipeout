#if SOCIAL_NETWORKS
using SocialNetworks.Services;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoDroid;
#endif
using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content.PM;
using Android.Text;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : BaseBindingActivity<LoginViewModel>
    {

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
                FindViewById<Button>(Resource.Id.ServerButton).Click += delegate { PromptServer(); };
            }
            else
            {
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Invisible;
            }

            if (!TinyIoCContainer.Current.Resolve<IAppSettings>().TwitterEnabled)
            {
                FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Invisible;
            }

            var linkResetPassword = FindViewById<UnderlineTextView>(Resource.Id.ForgotPasswordButton);

            linkResetPassword.SetTextColor(StyleManager.Current.NavigationTitleColor.ConvertToColor());

            Observable.FromEventPattern<EventHandler, EventArgs>(
                ev => ViewModel.LoginSucceeded += ev,
                ev => ViewModel.LoginSucceeded -= ev)
                .Subscribe(_ => Observable.Timer(TimeSpan.FromSeconds(2))
                    .Subscribe(__ => RunOnUiThread(Finish)));
        }

        private void PromptServer()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

            var input = new EditText(this)
            {
                InputType = InputTypes.TextFlagNoSuggestions,
                Text = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl
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
            GC.Collect();
        }
    }
}