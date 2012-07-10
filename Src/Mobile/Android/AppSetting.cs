using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client
{

    public class AppSettings : IAppSettings
    {

        public AppSettings(TaxiMobileApplication app)
        {
            App = app;
        }

        public TaxiMobileApplication App { get; set; }

        public static bool ErrorLogEnabled
        {
            get { return false; }
        }


        public static string ErrorLog
        {
            get { return ""; }
        }

        public static string SiteUrl
        {
            get { return "http://taxidiamondwebbook.dyndns.org/ibs/"; }
        }


        public static string PhoneNumber(int companyId)
        {
            if (companyId == 9) //Ouest
            {
                return "5146374444";
            }
            else if (companyId == 10) //Royal
            {
                return "5142743333";
            }
            else if (companyId == 11) //Candare			
            {
                return "5143361313";
            }
            else if (companyId == 13) //Veteran
            {
                return "5142736351";
            }
            else
            {
                return "5142736331";
            }
        }



        public static int GetLogo(int companyId)
        {
            if (companyId == 9) //Ouest
            {
                return Resource.Drawable.Logo_TaxiDiamondWest;  //"Assets/Logo_TaxiDiamondWest.png";
            }
            else if (companyId == 10) //Royal
            {
                return Resource.Drawable.Logo_TaxiRoyal;  //"Assets/Logo_TaxiRoyal.png";
            }
            else if (companyId == 11) //Candare			
            {
                return Resource.Drawable.Logo_TaxiCandare;  //"Assets/Logo_TaxiCandare.png";
            }
            else if (companyId == 13) //Veteran
            {
                return Resource.Drawable.Logo_TaxiVeteran;  //"Assets/Logo_TaxiVeteran.png";
            }
            else
            {
                return Resource.Drawable.Logo_TaxiDiamond;  //"Assets/TDLogo.png";
            }
        }
		
        public static string PhoneNumberDisplay(int companyId)
        {

            if (companyId == 9) //Ouest
            {
                return "(514)637-4444";
            }
            else if (companyId == 10) //Royal
            {
                return "(514)274-3333";
            }
            else if (companyId == 11) //Candare			
            {
                return "(514)336-1313";
            }
            else if (companyId == 13) //Veteran
            {
                return "(514)273-6351";
            }
            else
            {
                return "(514)273-6331";
            }
        }

        public int[] InvalidProviderIds
        {
            get { return new int[0]; }
        }


        public static string Version
        {
            get
            {
                var pInfo = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0);
                return pInfo.VersionName;
            }
        }




        public string ServiceUrl
        {        
            get { return "http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/"; }
        }
    }


}