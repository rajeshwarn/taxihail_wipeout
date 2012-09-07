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
            get { return "http://www.taxihail.com/"; }
        }


        public static string PhoneNumber(int providerId)
        {
            return "18666245330";           
        }



        public static int GetLogo(int companyId)
        {
            return Resource.Drawable.Logo_TaxiDiamond;  //"Assets/TDLogo.png";
        }
		
        public static string PhoneNumberDisplay(int companyId)
        {
            return "1.866.624.5330";
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



        //public string ServiceUrl
        //{            
        //    //get { return "http://demo.taxihail.biz/V1/api/"; }


        //    get { return "http://services.taxihail.com/TaxiHailDemo/V1/api/"; }
            
        //    //get { return "http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/"; }

        //}


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
                //var url = TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().Get<string>("TaxiHail.ServiceUrl");
                //if (string.IsNullOrEmpty(url))
                //{
                //    return string.Format(DefaultServiceUrl, DefaultServiceServer);
                //}
                //else
                //{
                //    return url;
                //}

                return "http://192.168.12.116/apcurium.MK.Web/api/"; 
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