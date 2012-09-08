using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppSettings : IAppSettings
    {
        
		public bool CanChooseProvider { get { return true; } }

        public static bool ErrorLogEnabled
        {
            get { return true; }
        }

        public static string ErrorLog
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "errorlog.txt"); }
        }
        
        public static string SiteUrl
        {
            get { return "http://www.taxihail.com/"; }
        }

        public static string PhoneNumber(int? providerId)
        {
            return "18666245330";           
        }

        public static string FacebookPageId
        {
            get { return "332870833414607"; }
        }
        
        public int[] InvalidProviderIds
        {
            get { return new int[0]; }
        }
        
        public static bool ShowNumberOfTaxi
        {
            get { return false;}
        }
        
        public static string GetLogo(int companyId)
        {            
            return "Assets/Logo.png";           
        }

        public static string PhoneNumberDisplay(int? companyId)
        {
            return "1.866.624.5330";
        }

        public static string Version
        {
            get
            {                
                NSObject nsVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion");
                string ver = nsVersion.ToString();
                return ver;         
            }
        }
        
        public string DefaultServiceUrl
        {
            get { return "http://services.taxihail.com/{0}/v1/api/"; }

        }

		public string DefaultServiceServer {
			get {
				return "taxihaildemo";
			}
		}
    
        public string ServiceUrl
        {
            get
            { 
                var url = TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().Get<string>("TaxiHail.ServiceUrl");
                if (string.IsNullOrEmpty(url))
                {
                    return string.Format(DefaultServiceUrl, DefaultServiceServer);
                }
                else
                {
                    return url;
                }                
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().Clear("TaxiHail.ServiceUrl");
                }
                else if (value.ToLower().StartsWith("http"))
                {
                    TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().Set<string>("TaxiHail.ServiceUrl", value);                
                }
                else
                {
                    TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().Set<string>("TaxiHail.ServiceUrl", string.Format(DefaultServiceUrl, value));                
                }

            }

        }


    }
}

