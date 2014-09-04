//VERY VERY VERY IMPORTANT NOTE!!!!
// Your package name MUST NOT start with an uppercase letter.
// Android does not allow permissions to start with an upper case letter
// If it does you will get a very cryptic error in logcat and it will not be obvious why you are crying!
// So please, for the love of all that is kind on this earth, use a LOWERCASE first letter in your Package Name!!!!
using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Cirrious.MvvmCross.ViewModels;
using PushSharp.Client;
using ServiceStack.Text;
using Android.Support.V4.App;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
//, ProtectionLevel = Android.Content.PM.Protection.Signature)]

[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is only needed for android versions 4.0.3 and below

[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IAppSettings _appSettings;
        private readonly Context _context;

        public PushNotificationService(Context context, IAppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }

        public void RegisterDeviceForPushNotifications(bool force = false)
        {
            //Check to ensure everything's setup right
            PushClient.CheckDevice(_context);
            PushClient.CheckManifest(_context);

            //Get the stored latest registration id
            var registrationId = PushClient.GetRegistrationId(_context);


            bool registered = !string.IsNullOrEmpty(registrationId);
            const string tag = "PushSharp-GCM";

            if (!registered)
            {
                Log.Info(tag, "Registering...");

                //Call to register
                var registerIdDebug = _appSettings.Data.SenderId;
                PushClient.Register(_context, registerIdDebug);
            }
            else
            {
                Log.Info(tag, "Already registered");
            }
        }

        public void SaveDeviceToken(string deviceToken)
        {
            throw new NotImplementedException("iOS only");
        }
    }

    //You must subclass this!
    [BroadcastReceiver(Permission = GCMConstants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new[] {GCMConstants.INTENT_FROM_GCM_MESSAGE}, Categories = new[] {"@PACKAGE_NAME@"})]
    [IntentFilter(new[] {GCMConstants.INTENT_FROM_GCM_REGISTRATION_CALLBACK}, Categories = new[] {"@PACKAGE_NAME@"})]
    [IntentFilter(new[] {GCMConstants.INTENT_FROM_GCM_LIBRARY_RETRY}, Categories = new[] {"@PACKAGE_NAME@"})]
    public class PushHandlerBroadcastReceiver : PushHandlerBroadcastReceiverBase<PushHandlerService>
    {
        public const string Tag = "PushSharp-GCM";
    }

    [Service] //Must use the service tag
    public class PushHandlerService : PushHandlerServiceBase,
        IUseServiceClient
    {
        public PushHandlerService()
            : base(TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().Data.SenderId)
        {

        }


        protected override void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(PushHandlerBroadcastReceiver.Tag, "GCM Registered: " + registrationId);
            //Send back to the server
            this.UseServiceClient<PushNotificationRegistrationServiceClient>(
                service => { service.Register(registrationId, PushNotificationServicePlatform.Android); });
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Verbose(PushHandlerBroadcastReceiver.Tag, "GCM Unregistered: " + registrationId);
            this.UseServiceClient<PushNotificationRegistrationServiceClient>(
                service => { service.Unregister(registrationId); });
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            Log.Info(PushHandlerBroadcastReceiver.Tag, "GCM Message Received!");

            var msg = new StringBuilder();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                    msg.AppendLine(key + "=" + intent.Extras.Get(key));
            }

            //Store the message
            var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
            var edit = prefs.Edit();
            edit.PutString("last_msg", msg.ToString());
            edit.Commit();

            if (intent != null)
            {
                Log.Info(PushHandlerBroadcastReceiver.Tag, "INTENT NOT NULL!");
                if (intent.Extras != null)
                {
                    var alert =
                        intent.Extras.KeySet()
                            .Where(key => key == "alert")
                            .Select(key => intent.Extras.Get(key).ToString())
                            .FirstOrDefault() ?? string.Empty;
                    var orderId =
                        intent.Extras.KeySet()
                            .Where(key => key == "orderId")
                            .Select(key => Guid.Parse(intent.Extras.Get(key).ToString()))
                            .FirstOrDefault();

                    var isParingNotification =
                        intent.Extras.KeySet()
                            .Where(key => key == "isPairingNotification")
                            .Select(key => bool.Parse(intent.Extras.Get(key).ToString()))
                            .FirstOrDefault();

                    CreateNotification(alert, "Tap to view...", orderId, isParingNotification);
                }
            }
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            Log.Warn(PushHandlerBroadcastReceiver.Tag, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.Error(PushHandlerBroadcastReceiver.Tag, "GCM Error: " + errorId);
        }

		static int notificationIntentcounter = 26; //to prevent exiting app to resend 0, we start at an arbitrary number

		private void CreateNotification(string title, string desc, Guid orderId, bool isPairingNotification)
		{
            Log.Info(PushHandlerBroadcastReceiver.Tag, "Creating notification!");

			//Create notification
			var notificationManager = GetSystemService(NotificationService) as NotificationManager;

			//Create an intent to show ui
			var uiIntent = new Intent(this, typeof (SplashActivity));
			var request = new MvxViewModelRequest(
				typeof (MvxNullViewModel),
				null,
				null,
				MvxRequestedBy.UserAction);
			var launchData = request.ToJson();

			uiIntent.PutExtra("MvxLaunchData", launchData);
			uiIntent.PutExtra("orderId", orderId.ToString());
		    uiIntent.PutExtra("isPairingNotification", isPairingNotification.ToString());

			var contentIntent = PendingIntent.GetActivity(this, notificationIntentcounter++,
				uiIntent, 0);

			var builder =
				new NotificationCompat.Builder(this)
					.SetSmallIcon(Resource.Drawable.notification_icon)
					.SetDefaults((int)NotificationDefaults.All)
					.SetAutoCancel(true)
					.SetContentTitle(title)
					.SetContentText(desc);

			builder.SetContentIntent(contentIntent);

			var notification = builder.Build ();

			//Show the notification
			if (notificationManager != null) {
				notificationManager.Notify (1, notification);
                Log.Info(PushHandlerBroadcastReceiver.Tag, "Notification sent");
			}
		}
    }
}