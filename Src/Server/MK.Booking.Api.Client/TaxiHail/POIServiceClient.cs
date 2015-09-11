#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Text;
using apcurium.MK.Common.Extensions;

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
            sb.Append("/references/pickuppoint");
            if(company.HasValue())
            {
                sb.Append(string.Format( "_{0}", company));
            }
            if(textMatch.HasValue())
            {
                sb.Append(string.Format("/{0}", textMatch));
            }
            sb.Append("?format=json");
            if( maxRespSize > 0 )
            {
                sb.Append(string.Format("&size={0}", maxRespSize));
            }
            var req = sb.ToString();
			Console.WriteLine (req);
            var pObject = Client.GetAsync<string>(req);
            return pObject;
        }

        public Task<string> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
            var sb = new StringBuilder();
            sb.Append("/references/airline");
			if (company.HasValue())
            {
                sb.Append(string.Format("_{0}", company));
            }
			if (textMatch.HasValue())
            {
                sb.Append(string.Format("/{0}", textMatch));
            }
            sb.Append("?format=json");
            if (maxRespSize > 0)
            {
                sb.Append(string.Format("&size={0}", maxRespSize));
            }
            sb.Append("&coreFieldsOnly=true");
            var req = sb.ToString();
			Console.WriteLine (req);
            var pObject = Client.GetAsync<string>(req);
            return pObject;
        }
    }
}