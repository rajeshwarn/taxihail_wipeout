using System;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using Foundation;
using System.Threading.Tasks;

using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social
{
	public class FacebookService: IFacebookService
    {
        private string _appId;

		public void Init()
		{
            try
            {
              
            }
            catch(Exception ex)
            {
                Logger.LogMessage("Facebook Init failed");
                Logger.LogError(ex);
            }
		}

		public void PublishInstall()
		{
            try
            {
              
            }
            catch(Exception ex)
            {
                Logger.LogMessage("Facebook PublishInstall failed");
                Logger.LogError(ex);
            }
		}

		public Task Connect()
		{
			

			var tcs = new TaskCompletionSource<object>();

			return tcs.Task;
		}

		public void Disconnect()
		{
			 
		}

		public Task<FacebookUserInfo> GetUserInfo()
		{
			var tcs = new TaskCompletionSource<FacebookUserInfo>();
			 
			return tcs.Task;
		}
    }
}

