using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System.IO;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "Settings", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			
            SetContentView(Resource.Layout.Settings);
		
			FindViewById<TextView>(Resource.Id.version).Text += AppSettings.Version;
			
			FindViewById<Button>(Resource.Id.AboutButton).Click += new EventHandler(About_Click);
			FindViewById<Button>(Resource.Id.SignOutButton).Click += new EventHandler(Logout_Click);
			FindViewById<Button>(Resource.Id.TechSupportButton).Click += new EventHandler(ReportProblem_Click);
			FindViewById<Button>(Resource.Id.ChangeDefaultRideSettings).Click += new EventHandler(ChangeDefaultRideSettings_Click);
			FindViewById<TextView>(Resource.Id.CallCompanyButton).Click += new EventHandler(CallCie_Click);
		}
		
		private void About_Click(object sender, EventArgs e)
        {
			var intent = new Intent().SetClass(this, typeof(AboutActivity));
			StartActivity(intent);
		}
		
		private void CallCie_Click(object sender, EventArgs e)
        {
            //TODO:Fix this
            RunOnUiThread(() => AlertDialogHelper.Show(this, "", AppSettings.PhoneNumberDisplay(AppContext.Current.LoggedUser.Settings.ProviderId), "Call", CallCie, "Cancel", delegate { }));
		}
		
		private void CallCie( object sender, EventArgs e )
		{
            Intent callIntent = new Intent(Intent.ActionCall, Android.Net.Uri.Parse("tel:" + AppSettings.PhoneNumberDisplay(AppContext.Current.LoggedUser.Settings.ProviderId)));
            //TODO:Fix this
            //callIntent.SetData(Android.Net.Uri.Parse("tel:" + AppSettings.PhoneNumber(AppContext.Current.LoggedUser.Settings.Company)));
			StartActivity(callIntent);
				
		}
	
		private void Logout_Click( object sender, EventArgs e )
		{
            AppContext.Current.SignOut();
            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            if (facebook.IsConnected)
            {
                facebook.SetCurrentContext(this);
                facebook.Disconnect();
            }

		    var twitterService = TinyIoC.TinyIoCContainer.Current.Resolve<ITwitterService>();
            if(twitterService.IsConnected)
            {
                twitterService.Disconnect();
            }
           
            RunOnUiThread(() =>
            {
                Finish();
                StartActivity(typeof(LoginActivity));
            });				
		}
		
		private void ReportProblem_Click( object sender, EventArgs e )
		{


			ThreadHelper.ExecuteInThread( this, () => {
				Intent emailIntent = new Intent( Intent.ActionSend );
                
				emailIntent.SetType( "application/octet-stream" );
				emailIntent.PutExtra( Intent.ExtraEmail, new String[]{Resources.GetString( Resource.String.SupportEmail )} );
				emailIntent.PutExtra( Intent.ExtraCc, new String[]{AppContext.Current.LoggedInEmail} );
				emailIntent.PutExtra( Intent.ExtraSubject, Resources.GetString( Resource.String.TechSupportEmailTitle ) );
				
				//following line is for test purposes only.  Need to be removed when tested and also remove AboutAssets.txt from Assets (set action to None or remove completely)
				emailIntent.PutExtra( Intent.ExtraStream,  Android.Net.Uri.Parse( @"file:///" +  LoggerImpl.LogFilename )); // @"file:///android_asset/AboutAssets.txt" ) );
				
				if( AppSettings.ErrorLogEnabled && File.Exists( AppSettings.ErrorLog ) )
				{
					emailIntent.PutExtra( Intent.ExtraStream,  Android.Net.Uri.Parse( AppSettings.ErrorLog ) );
				}
				try {
					StartActivity( Intent.CreateChooser( emailIntent, "Send mail...") );
                    LoggerImpl.FlushNextWrite();
				}
				catch ( Android.Content.ActivityNotFoundException ex) {
					RunOnUiThread( () => Toast.MakeText( this, Resources.GetString( Resource.String.NoMailClient ), ToastLength.Short ).Show() );
				}
			}, false );
		}	
		
		private void ChangeDefaultRideSettings_Click( object sender, EventArgs e )
		{
			var intent = new Intent().SetClass(this, typeof(RideSettingsActivity));
			StartActivity(intent);
		}	
		
		protected override void OnResume ()
		{
			base.OnResume ();
			
			if( AppContext.Current.LoggedUser != null )
			{
				FindViewById<TextView>(Resource.Id.signedInInfoText).Text = string.Format( Resources.GetString( Resource.String.SettingViewLoginInfo ), AppContext.Current.LoggedUser.Name );
			}
			var callCieBtn = FindViewById<TextView>(Resource.Id.CallCompanyButton);                        
            callCieBtn.Text = Resources.GetString( Resource.String.CallCompanyButton );
		}
		
	
    }
}