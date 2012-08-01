using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using apcurium.MK.Booking.Mobile.Infrastructure;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class AppSettings : IAppSettings
	{
		
		

		public static bool ErrorLogEnabled {
			get { return true; }
		}


		public static string ErrorLog {
			get { return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "errorlog.txt"); }
		}

		
		 public static string SiteUrl
        {
            get { return "http://www.taxihail.com/"; }
        }


        public static string PhoneNumber(int providerId)
        {
            return "18666245330";           
        }

		public static string FacebookPageId {
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
		
		public static string GetLogo( int companyId ) {
			
			return "Assets/Logo.png"; 			
		}

		 public static string PhoneNumberDisplay(int companyId)
        {
            return "1.866.624.5330";
        }


		public static string Version {
			get {
				
				NSObject nsVersion = NSBundle.MainBundle.ObjectForInfoDictionary ("CFBundleVersion");
				string ver = nsVersion.ToString ();
				return ver;			
			}
		}
		
		
		
				
	
        public string ServiceUrl
        {
            //get { return "http://192.168.12.125/apcurium.MK.Web/api/"; }
            //get { return "http://192.168.1.12/apcurium.MK.Web/api/"; }
            get { return "http://project.apcurium.com/TaxiHailV2/api/"; }

        }

	}
}

