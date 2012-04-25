using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using MobileTaxiApp.Infrastructure;

namespace TaxiMobileApp
{
	public class AppSettings : IAppSettings
	{
		
		

		public static bool ErrorLogEnabled {
			get { return true; }
		}


		public static string ErrorLog {
			get { return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "errorlog.txt"); }
		}

		
		public static string SiteUrl {
			get { return "http://taxidiamondwebbook.dyndns.org/ibs/"; }		
		}
		

		public static string PhoneNumber( int companyId ) {
			if ( companyId == 9 ) //Ouest
			{
				return "5146374444";
			}
			else if ( companyId == 10) //Royal
			{
				return "5142743333";
			}
			else if ( companyId == 11) //Candare			
			{
				return "5143361313";
			}
			else if ( companyId == 13) //Veteran
			{
				return "5142736351";
			}
			else
			{
			return "5142736331"; 
			}
		}
		
		
		
		public static string GetLogo( int companyId ) {
			if ( companyId == 9 ) //Ouest
			{
				return "Assets/Logo_TaxiDiamondWest.png";
			}
			else if ( companyId == 10) //Royal
			{
				return "Assets/Logo_TaxiRoyal.png";
			}
			else if ( companyId == 11) //Candare			
			{
				return "Assets/Logo_TaxiCandare.png";
			}
			else if ( companyId == 13) //Veteran
			{
				return "Assets/Logo_TaxiVeteran.png";
			}
			else
			{
			return "Assets/TDLogo.png"; 
			}
		}
		public static string PhoneNumberDisplay( int companyId ) {
			
			if ( companyId == 9 ) //Ouest
			{
				return "(514)637-4444";
			}
			else if ( companyId == 10) //Royal
			{
				return "(514)274-3333";
			}
			else if ( companyId == 11) //Candare			
			{
				return "(514)336-1313";
			}
			else if ( companyId == 13) //Veteran
			{
				return "(514)273-6351";
			}
			else
			{
				return "(514)273-6331"; 
			}
		}


		public static string Version {
			get {
				
				NSObject nsVersion = NSBundle.MainBundle.ObjectForInfoDictionary ("CFBundleVersion");
				string ver = nsVersion.ToString ();
				return ver;			
			}
		}
		
		
		
				
		public string ServiceUrl {
//			get { return "http://taxidiamondwebbook.dyndns.org/TaxiMobileWebService/"; }				
			get { return "http://taxidiamondwebbook.dyndns.org/TaxiMobileWebServiceProd/"; }				
		}

	}
}

