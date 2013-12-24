using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Util;
using Android.Content;
using GCMSharp.Client;
using Android.App;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using System.Text;
using apcurium.MK.Common.Enumeration;
using System.Dynamic.Utils;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Activities;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class PushNotificationService: IPushNotificationService
	{
		private readonly Context _context;
        private readonly IConfigurationManager _configManager;
		public PushNotificationService (Context context, IConfigurationManager configManager)
		{
			this._context = context;
            _configManager = configManager;
		}

		public void RegisterDeviceForPushNotifications (bool force = false)
		{
			//Check to ensure everything's setup right
			GCMSharp.Client.GCMRegistrar.CheckDevice(_context);
			GCMSharp.Client.GCMRegistrar.CheckManifest(_context);

			if (force) {
				GCMSharp.Client.GCMRegistrar.ClearRegistrationId(_context);
			}
			
			//Get the stored latest registration id
			var registrationId = GCMSharp.Client.GCMRegistrar.GetRegistrationId(_context);
			
			
			bool registered = !string.IsNullOrEmpty(registrationId);
			const string TAG = "PushSharp-GCM";
			
			if (!registered)
			{
				Log.Info(TAG, "Registering...");
				
				//Call to register
                var registerIdDebug =  _configManager.GetSetting( "GCM.SenderId" );
                GCMSharp.Client.GCMRegistrar.Register(_context, registerIdDebug);
			}
			else {
				Log.Info(TAG, "Already registered");
			}
		}

		public void SaveDeviceToken (string deviceToken)
		{
			throw new NotImplementedException ("iOS only");
		}
	}

	//You must subclass this!
	public class SampleBroadcastReceiver : GCMBroadcastReceiver<GCMIntentService>
	{
		//IMPORTANT: Change this to your own Sender ID!
		//The SENDER_ID is your Google API Console App Project ID.
		//  Be sure to get the right Project ID from your Google APIs Console.  It's not the named project ID that appears in the Overview,
		//  but instead the numeric project id in the url: eg: https://code.google.com/apis/console/?pli=1#project:785671162406:overview
		//  where 785671162406 is the project id, which is the SENDER_ID to use!
        //public const string SENDER_ID = "385816297456"; 
		
		public const string TAG = "PushSharp-GCM";
	}
	
	[Service] //Must use the service tag
	public class GCMIntentService : GCMBaseIntentService,
		IUseServiceClient
	{
        public GCMIntentService() : base(TinyIoC.TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("GCM.SenderId")) { }
		
        
		protected override void OnRegistered (Context context, string registrationId)
		{
			Log.Verbose(SampleBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
			//Send back to the server
			this.UseServiceClient<PushNotificationRegistrationServiceClient>(service =>
			                                                                 {
				service.Register(registrationId, PushNotificationServicePlatform.Android);
			});

		}
		
		protected override void OnUnRegistered (Context context, string registrationId)
		{
			Log.Verbose(SampleBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);
			this.UseServiceClient<PushNotificationRegistrationServiceClient>(service =>
			                                                                 {
				service.Unregister(registrationId);
			});

		}
		
		protected override void OnMessage (Context context, Intent intent)
		{
			Log.Info(SampleBroadcastReceiver.TAG, "GCM Message Received!");
			
			var msg = new StringBuilder();
			
			if (intent != null && intent.Extras != null)
			{
				foreach (var key in intent.Extras.KeySet())
					msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
			}
			
			//Store the message
			var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
			var edit = prefs.Edit();
			edit.PutString("last_msg", msg.ToString());
			edit.Commit();

			var alert = intent.Extras.KeySet().Where(key => key == "alert").Select(key => intent.Extras.Get(key).ToString()).FirstOrDefault() ?? string.Empty;
			var orderId = intent.Extras.KeySet ().Where(key => key == "orderId").Select(key => Guid.Parse(intent.Extras.Get (key).ToString())).FirstOrDefault();

			createNotification(alert, "Tap to view...", orderId);
		}
		
		protected override bool OnRecoverableError (Context context, string errorId)
		{
			Log.Warn(SampleBroadcastReceiver.TAG, "Recoverable Error: " + errorId);
			
			return base.OnRecoverableError (context, errorId);
		}
		
		protected override void OnError (Context context, string errorId)
		{
			Log.Error(SampleBroadcastReceiver.TAG, "GCM Error: " + errorId);
		}
		
		void createNotification (string title, string desc, Guid orderId)
		{
			//Create notification
			var notificationManager = GetSystemService (Context.NotificationService) as NotificationManager;

			//Create an intent to show ui
			var uiIntent = new Intent (this, typeof(SplashActivity));
		    var request = new MvxShowViewModelRequest(
		        typeof (MvxNullViewModel),
		        new Dictionary<string, string>(),
		        true,
		        MvxRequestedBy.UserAction);
            var launchData = request.ToJson();
			
			uiIntent.PutExtra ("MvxLaunchData", launchData);
			uiIntent.PutExtra ("orderId", orderId.ToString());

			
			//Create the notification
			var notification = new Notification (Resource.Drawable.notification_icon, title);

			//Auto cancel will remove the notification once the user touches it
			notification.Flags = NotificationFlags.AutoCancel;
			notification.Defaults = NotificationDefaults.All;
		
			//Set the notification info
			//we use the pending intent, passing our ui intent over which will get called
			//when the notification is tapped.
			notification.SetLatestEventInfo (this,
		                                title,
		                                desc,
		                                PendingIntent.GetActivity (this, 0, uiIntent, 0));
		
			//Show the notification
			notificationManager.Notify (1, notification);

		}
	}
}

