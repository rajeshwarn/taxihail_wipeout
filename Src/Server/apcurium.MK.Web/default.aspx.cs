using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web
{
    public partial class _default : PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        protected string DefaultLatitude { get; private set; }
        protected string DefaultLongitude { get; private set; }
        protected string DefaultPhoneNumber { get; private set; }
        protected bool IsAuthenticated { get; private set; }
        protected string FacebookAppId { get; private set; }
        protected string FacebookEnabled { get; private set; }
        protected string HideDispatchButton { get; private set; }
        protected string GeolocPopularRange { get; private set; }
        protected string GeolocSearchFilter { get; private set; }
        protected string GeolocSearchRegion { get; private set; }
        protected string GeolocSearchBounds { get; private set; }
        protected string EstimateEnabled { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");
            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            DefaultPhoneNumber = config.GetSetting("DefaultPhoneNumberDisplay");
            IsAuthenticated = base.UserSession.IsAuthenticated;
            FacebookAppId = config.GetSetting("FacebookAppId");
            FacebookEnabled = config.GetSetting("FacebookEnabled");
            HideDispatchButton = config.GetSetting("Client.HideCallDispatchButton");
            GeolocPopularRange = config.GetSetting("GeoLoc.PopularAddress.Range");
            EstimateEnabled = config.GetSetting("Client.ShowEstimate");

            var filters = config.GetSetting("GeoLoc.SearchFilter").Split('&');
            GeolocSearchFilter = filters.Length > 0 
                ? Uri.UnescapeDataString(filters[0]).Replace('+', ' ')
                : "{0}";
            GeolocSearchRegion = FindParam(filters, "region");
            GeolocSearchBounds = FindParam(filters, "bounds");

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