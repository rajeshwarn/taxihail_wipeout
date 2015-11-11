using System;
using System.IO;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using Foundation;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostics
{
    public static class Logger
    {
        public static void LogError(Exception ex)
        {
            new LoggerWrapper().LogError(ex);
        }

        public static void LogMessage(string message)
        {
            new LoggerWrapper().LogMessage(message);
        }
    }

    public class LoggerWrapper : BaseLogger
    {
        private string BaseDir 
        {
            get
            {
                if (UIHelper.IsOS8orHigher)
                {
                    var docs = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User) [0];
                    return docs.Path;
                }
                else
                {
                    return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library");
                }
            }
        }

		protected override string GetBaseDirectory()
        {
            return BaseDir;
        }

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