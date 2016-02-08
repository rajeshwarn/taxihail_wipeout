#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Text;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class POIServiceClient : BaseServiceClient
    {
        public POIServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

	    public Task<PickupPoint[]> GetPOIRefPickupList(string company, string textMatch, int maxRespSize)
        {
			var request = GetParameters(company, textMatch, maxRespSize, "pickuppoint").ToString();

            return Client.GetAsync<PickupPoint[]>(request, logger: Logger);
        }

		public Task<Airline[]> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
			var request = GetParameters(company, textMatch, maxRespSize, "airline")
				.Append("&coreFieldsOnly=true")
				.ToString();

            return Client.GetAsync<Airline[]>(request, logger: Logger);
        }

	    private static StringBuilder GetParameters(string company, string textMatch, int maxRespSize,string endpoint)
	    {
		    var stringBuilder = new StringBuilder();
			stringBuilder.Append("/references/").Append(endpoint);
		    if (company.HasValue())
		    {
			    stringBuilder.Append(string.Format("_{0}", company));
		    }
		    if (textMatch.HasValue())
		    {
			    stringBuilder.Append(string.Format("/{0}", textMatch));
		    }
		    stringBuilder.Append("?format=json");
		    if (maxRespSize > 0)
		    {
			    stringBuilder.Append(string.Format("&size={0}", maxRespSize));
		    }
		    return stringBuilder;
	    }
    }
}