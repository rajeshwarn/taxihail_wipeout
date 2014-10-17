using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
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
        private readonly string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library");

        public override string GetErrorLogPath ()
        {
            return Path.Combine (BaseDir, "errorlog.txt");
        }

        protected override void Write (string message)
        {
            try
            {
                if (!Directory.Exists(BaseDir))
                {
                    Directory.CreateDirectory(BaseDir);
                }

                var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
                var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();
                var account = TinyIoCContainer.Current.CanResolve<IAccountService> () 
                    ? TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount
                    : null;   
                var user = account == null
                    ? @" N\A "
                    : account.Email;

                message += string.Format(" by : {0} with version {1} - company {2} - platform {3}",
                    user,
                    packageInfo.Version,
                    settings.TaxiHail.ApplicationName,
                    packageInfo.PlatformDetails);

                Console.WriteLine (message);            

                DeleteLogIfNecessary();

                var filePath = GetErrorLogPath();
                if (File.Exists (filePath))
                {
                    var f = new FileInfo (filePath);
                    var lenKb = f.Length / 1024;
                    if (lenKb > 375)
                    {
                        f.Delete();
                    }
                }

                File.AppendAllLines(filePath, new[] { message });

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        protected override void SendToInsights (Exception ex)
        {
            #if DEBUG
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
            var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();

            var account = TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount;
            var email = account != null 
                ? account.Email 
                : settings.Insights.UnknownUserIdentifier;

            var identification = new Dictionary<string, string>
            {
                { "ApplicationVersion", packageInfo.Version },
                { "Company", settings.TaxiHail.ApplicationName },
            };

            Xamarin.Insights.Identify(email, identification);
            Xamarin.Insights.Report(ex);
            #endif
        }
    }
}

