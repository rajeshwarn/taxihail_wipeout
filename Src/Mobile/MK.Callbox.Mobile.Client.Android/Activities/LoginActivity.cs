using System;
using apcurium.MK.Booking.Mobile.Client;
using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class LoginActivity : BaseBindingActivity<CallboxLoginViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Login);

			if (ViewModel.Settings.CanChangeServiceUrl)
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
#if DEBUG
            FindViewById<EditText>(Resource.Id.Username).Text = "john@taxihail.com";
            FindViewById<EditText>(Resource.Id.Password).Text = "password";
#endif 
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

                if (serviceUrl != null)
                {
                    ViewModel.SetServerUrl(serviceUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignIn; }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}