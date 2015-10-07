#region

using System;
using System.Linq;
using System.Reflection;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Microsoft.Practices.ServiceLocation;
using ServiceStack.Text;
using apcurium.MK.Common;
using System.Globalization;

#endregion

namespace apcurium.MK.Web.admin
{
    public partial class _default : PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        protected string ApplicationVersion { get; private set; }
        protected string DefaultLatitude { get; private set; }
        protected string DefaultLongitude { get; private set; }
        protected bool IsAuthenticated { get; private set; }
        protected bool IsSuperAdmin { get; private set; }
        protected string GeolocSearchFilter { get; private set; }
        protected string GeolocSearchRegion { get; private set; }
        protected string GeolocSearchBounds { get; private set; }
        protected bool IsTaxiHailPro { get; private set; }
        protected bool IsAdmin { get; private set; }
        protected bool IsNetworkEnabled { get; private set; }
        protected string Languages { get; private set; }

		protected string CountryCodes { get; private set; }

		protected string DefaultCountryCode { get; private set; }

		protected string CurrentAccountID { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IServerSettings>();

            ApplicationKey = config.ServerData.TaxiHail.ApplicationKey;
            ApplicationName = config.ServerData.TaxiHail.ApplicationName;

            DefaultLatitude = config.ServerData.GeoLoc.DefaultLatitude.ToString();
            DefaultLongitude = config.ServerData.GeoLoc.DefaultLongitude.ToString();
            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();

            IsAuthenticated = base.UserSession.IsAuthenticated;
            IsSuperAdmin = UserSession.HasPermission(RoleName.SuperAdmin);
            IsAdmin = UserSession.HasPermission(RoleName.Admin);
			CurrentAccountID = UserSession.UserAuthId;

            IsNetworkEnabled = config.ServerData.Network.Enabled;

            var languages = Enum.GetNames(typeof(SupportedLanguages)).ToList();
            Languages = JsonSerializer.SerializeToString(languages, languages.GetType());

            var filters = config.ServerData.GeoLoc.SearchFilter.Split('&');
            GeolocSearchFilter = filters.Length > 0
                ? Uri.UnescapeDataString(filters[0]).Replace('+', ' ')
                : "{0}";
            GeolocSearchRegion = FindParam(filters, "region");
            GeolocSearchBounds = FindParam(filters, "bounds");

            if (!base.UserSession.HasPermission(RoleName.Support))
            {
                Response.Redirect("~");
            }


			CountryCodes = Newtonsoft.Json.JsonConvert.SerializeObject(CountryCode.CountryCodes);

			CultureInfo defaultCultureInfo = CultureInfo.GetCultureInfo(config.ServerData.PriceFormat);

			if (defaultCultureInfo != null)
			{
				DefaultCountryCode = (new RegionInfo(defaultCultureInfo.LCID)).TwoLetterISORegionName;
			}
			else
			{
				DefaultCountryCode = "CA";
			}

            IsTaxiHailPro = config.ServerData.IsTaxiHailPro;
        }

        protected string FindParam(string[] filters, string param)
        {
            var pair = filters.FirstOrDefault(x => x.StartsWith(param + "="));

            return pair == null
                ? string.Empty
                : Uri.UnescapeDataString(pair.Split('=')[1]);
        }
    }
}