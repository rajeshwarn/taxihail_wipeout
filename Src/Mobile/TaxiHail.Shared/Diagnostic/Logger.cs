using System;
using System.IO;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using Environment = Android.OS.Environment;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;

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
        private readonly string _baseDir = Path.Combine (Environment.ExternalStorageDirectory.ToString (), "TaxiHail");

        private string GetCompanyName()
        {
            IAppSettings settings;

            if (!TinyIoCContainer.Current.TryResolve(out settings))
            {
                return "Unknown";
            }

            return settings.Data.TaxiHail.ApplicationName.HasValueTrimmed()
                ? settings.Data.TaxiHail.ApplicationName
                : "Unknown";
        }

		protected override string GetBaseDirectory()
		{
			return _baseDir;
		}

		protected override string GetMessageBase()
		{
            var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();

		    return " by : {0} with version {1} - company {2} - platform {3}".InvariantCultureFormat(
                    GetAccount().SelectOrDefault(account => account.Email, @"N\A"),
                    packageInfo.Version,
                    GetCompanyName(),
		            packageInfo.PlatformDetails);
		}

        private static Account GetAccount()
        {
            IAccountService accountService;

            return TinyIoCContainer.Current.TryResolve(out accountService) 
                ? accountService.CurrentAccount 
                : null;
        }

   }
}