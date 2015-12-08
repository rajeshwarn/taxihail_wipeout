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
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;

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