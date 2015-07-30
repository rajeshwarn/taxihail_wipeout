#region

using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using System;
using System.Text;
using ServiceStack.ServiceClient.Web;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class POIServiceClient : BaseServiceClient
    {
        public POIServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<string> GetPOIRefPickupList(string company, string textMatch, int maxRespSize)
        {
            var sb = new StringBuilder();
            sb.Append("/../reference/pickuppoint");
            if( company != null && company != string.Empty )
            {
                sb.Append(string.Format( "_{0}", company));
            }
            if( textMatch != null && textMatch != string.Empty )
            {
                sb.Append(string.Format("/{0}", textMatch));
            }
            sb.Append("?format=json");
            if( maxRespSize > 0 )
            {
                sb.Append(string.Format("&size={0}", maxRespSize));
            }
            var req = sb.ToString();
            var pObject = Client.GetAsync<string>(req);
            return pObject;
        }

        public Task<string> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
            var sb = new StringBuilder();
            sb.Append("/../reference/airline");
            if (company != null && company != string.Empty)
            {
                sb.Append(string.Format("_{0}", company));
            }
            if (textMatch != null && textMatch != string.Empty)
            {
                sb.Append(string.Format("/{0}", textMatch));
            }
            sb.Append("?format=json");
            if (maxRespSize > 0)
            {
                sb.Append(string.Format("&size={0}", maxRespSize));
            }
            sb.Append(string.Format("&coreFieldsOnly=true"));
            var req = sb.ToString();
            var pObject = Client.GetAsync<string>(req);
            return pObject;
        }
    }
}