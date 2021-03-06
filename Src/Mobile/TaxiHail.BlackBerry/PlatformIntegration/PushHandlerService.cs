//VERY VERY VERY IMPORTANT NOTE!!!!
// Your package name MUST NOT start with an uppercase letter.
// Android does not allow permissions to start with an upper case letter
// If it does you will get a very cryptic error in logcat and it will not be obvious why you are crying!
// So please, for the love of all that is kind on this earth, use a LOWERCASE first letter in your Package Name!!!!

using Android.Content;
using Android.Util;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Enumeration;
using PushSharp.Client;


namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    /*
     * PARTIAL CLASS : the other part of the code is situated in the TaxiHail.Shared Project 
    */
    public partial class PushHandlerService : PushHandlerServiceBase,
        IUseServiceClient
    {
        protected override void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(PushHandlerBroadcastReceiver.Tag, "GCM Registered: " + registrationId);
            //Send back to the server
            this.UseServiceClient<PushNotificationRegistrationServiceClient>(
                service => { service.Register(registrationId, PushNotificationServicePlatform.BlackBerry); });
        }
    }
}