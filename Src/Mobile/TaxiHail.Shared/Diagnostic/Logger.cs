using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using Environment = Android.OS.Environment;
using System.Collections.Generic;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
    public static class Logger
    {
        public static void LogError(Exception ex)
        {
            new LoggerImpl().LogError(ex);
        }

        public static void LogMessage(string message)
        {
            new LoggerImpl().LogMessage(message);
        }
    }
        
    public class LoggerImpl : BaseLogger
   {
        private readonly string BaseDir = Path.Combine (Environment.ExternalStorageDirectory.ToString (), "TaxiHail");

        private TaxiHailSetting GetSettings()
        {
            try
            {
                var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
                return settings.Data;
            }
            catch
            {
                return null;
            }
        }

        private string GetCompanyName()
        {
            var settings = GetSettings();
            return settings != null 
                ? settings.TaxiHail.ApplicationName
                : "Unknown";
        }

		protected override string GetBaseDirectory()
		{
			return BaseDir;
		}

		protected override string GetMessageBase()
		{
			var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();
			var account = TinyIoCContainer.Current.CanResolve<IAccountService>()
				? TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount : null;

			return " by : " + (account == null ? @" N\A " : account.Email)
				+ string.Format("with version {0} - company {1} - platform {2}", packageInfo.Version, GetCompanyName(), packageInfo.PlatformDetails);
		}
   }
}