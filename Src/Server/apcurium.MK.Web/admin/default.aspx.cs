#region

using System;
using System.Linq;
using System.Reflection;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using Microsoft.Practices.ServiceLocation;

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

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");

            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();

            IsAuthenticated = base.UserSession.IsAuthenticated;
            IsSuperAdmin = UserSession.HasPermission(RoleName.SuperAdmin);


            var filters = config.GetSetting("GeoLoc.SearchFilter").Split('&');
            GeolocSearchFilter = filters.Length > 0
                ? Uri.UnescapeDataString(filters[0]).Replace('+', ' ')
                : "{0}";
            GeolocSearchRegion = FindParam(filters, "region");
            GeolocSearchBounds = FindParam(filters, "bounds");

            if (!base.UserSession.HasPermission(RoleName.Admin))
            {
                Response.Redirect("~");
            }
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