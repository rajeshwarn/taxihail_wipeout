
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
using Android.Util;
using GCMSharp.Client;
using MK.Booking.Api.Client.Android;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Services
{
	//You must subclass this!
	[BroadcastReceiver(Permission=GCMConstants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] { GCMConstants.INTENT_FROM_GCM_MESSAGE },
	Categories = new string[] { "com.apcurium.MK.TaxiHailDemo" })]
	[IntentFilter(new string[] { GCMConstants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
	Categories = new string[] { "com.apcurium.MK.TaxiHailDemo" })]
	[IntentFilter(new string[] { GCMConstants.INTENT_FROM_GCM_LIBRARY_RETRY },
	Categories = new string[] { "com.apcurium.MK.TaxiHailDemo" })]
	public class SampleBroadcastReceiver : GCMBroadcastReceiver<GCMIntentService>
	{
		//IMPORTANT: Change this to your own Sender ID!
		//The SENDER_ID is your Google API Console App Project ID.
		//  Be sure to get the right Project ID from your Google APIs Console.  It's not the named project ID that appears in the Overview,
		//  but instead the numeric project id in the url: eg: https://code.google.com/apis/console/?pli=1#project:785671162406:overview
		//  where 785671162406 is the project id, which is the SENDER_ID to use!
		public const string SENDER_ID = "385816297456"; 
		
		public const string TAG = "PushSharp-GCM";
	}
	
	[Service] //Must use the service tag
	public class GCMIntentService : GCMBaseIntentService, IUseServiceClient
	{
		public GCMIntentService() : base(SampleBroadcastReceiver.SENDER_ID) {}

		protected override void OnRegistered (Context context, string registrationId)
		{
			Log.Verbose(SampleBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
			//Send back to the server
			this.UseServiceClient<PushNotificationsServiceClient>(service =>
			{
				//service.Register(registrationId);
			});

			//createNotification("PushSharp-GCM Registered...", "The device has been Registered, Tap to View!");
		}
		
		protected override void OnUnRegistered (Context context, string registrationId)
		{
			Log.Verbose(SampleBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);
			this.UseServiceClient<PushNotificationsServiceClient>(service =>
			{
				//service.Unregister(registrationId);
			});
			
			//createNotification("PushSharp-GCM Unregistered...", "The device has been unregistered, Tap to View!");
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
			
			createNotification("PushSharp-GCM Msg Rec'd", "Message Received for C2DM-Sharp... Tap to View!");
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
		
		void createNotification(string title, string desc)
		{
			//Create notification
			var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
			
			//Create an intent to show ui
			var uiIntent = new Intent(this, typeof(AlertDialogActivity));
			
			//Create the notification
			var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);
			
			//Auto cancel will remove the notification once the user touches it
			notification.Flags = NotificationFlags.AutoCancel;
			
			//Set the notification info
			//we use the pending intent, passing our ui intent over which will get called
			//when the notification is tapped.
			notification.SetLatestEventInfo(this,
			                                title,
			                                desc,
			                                PendingIntent.GetActivity(this, 0, uiIntent, 0));
			
			//Show the notification
			notificationManager.Notify(1, notification);
		}
	}
}

