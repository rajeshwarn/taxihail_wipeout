using System;
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

        private void PromptServer()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

	        var settings = ViewModel.Settings;
			var input = new EditText(this)
			{
				Text = settings.ServiceUrl
			};

	        alert.SetView(input);

            alert.SetPositiveButton("Ok", (s, e) =>
            {
                var serverUrl = input.Text;
				settings.ServiceUrl = serverUrl;
	
            });

            alert.SetNegativeButton("Cancel", (s, e) =>
            {

            });

            alert.Show();
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