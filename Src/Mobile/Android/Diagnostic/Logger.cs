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
                    GetCompanyName(),
                    packageInfo.PlatformDetails);

                Console.WriteLine(message);

                DeleteLogIfNecessary();

                var filePath = GetErrorLogPath ();
                if (File.Exists(filePath))
                {
                    var f = new FileInfo(filePath);
                    var lenKb = f.Length/1024;
                    if (lenKb > 375)
                    {
                        File.Delete(filePath);
                    }
                }

                File.AppendAllLines(filePath, new[] { message });
            }
            catch(Exception e)
            {
                Console.WriteLine (e.Message);
            }
        }

        protected override void SendToInsights (Exception ex)
        {
            #if !DEBUG
            var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();

            var account = TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount;
            var settings = GetSettings();
            var unknownUserIdentifier = settings != null 
                ? settings.Insights.UnknownUserIdentifier
                : new TaxiHailSetting().Insights.UnknownUserIdentifier; //default value if we can't load the settings

            var email = account != null 
                ? account.Email 
                : unknownUserIdentifier;

            var identification = new Dictionary<string, string>
            {
                { "ApplicationVersion", packageInfo.Version },
                { "Company", GetCompanyName() },
            };

            Xamarin.Insights.Identify(email, identification);
            Xamarin.Insights.Report(ex);
            #endif
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
    }
}