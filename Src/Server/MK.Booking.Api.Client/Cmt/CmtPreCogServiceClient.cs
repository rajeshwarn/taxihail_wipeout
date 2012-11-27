using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtPreCogServiceClient : CmtBaseServiceClient
    {

        private readonly string[] propertiesToExcludeIfNull = new[]{ "LinkedVehiculeId" };
        private readonly string[] propertiesToExcludeINoGps = new[] { "GpsLat", "GpsLon", "GpsSpeed", "GpsBearing", "GpsLat", "GpsAccuracy", "GpsAltitude" };


        public CmtPreCogServiceClient(string url) : base(url)
        {
        }

        public CmtPreCogServiceClient(string url, CmtAuthCredentials cmtAuthCredentials) : base(url, cmtAuthCredentials)
        {
        }

        public PreCogResponse Send(PreCogRequest request, bool activeGps)
        {
            var req = string.Format("/precog/{0}?{1}", Credentials.AccountId, FormatRequest(request, activeGps));
            var response = Client.Get<PreCogResponse>(req);
            return response;
        }

        private string FormatRequest(PreCogRequest request, bool activeGps)
        {
            var keyValues = request.ToStringDictionary();

            IEnumerable<string> propertiesFilter = propertiesToExcludeIfNull;
            if (!activeGps)
            {
                propertiesFilter = propertiesToExcludeIfNull.Concat(propertiesToExcludeINoGps);
            }

            //remove properties when null and sould not be send
            var parameterToRemoved = keyValues.Where(x => propertiesFilter.Contains(x.Key) && x.Value == null).Select(x => x.Key).ToList();
            parameterToRemoved.ForEach(x => keyValues.Remove(x));

            

            var queryString = keyValues.Skip(1).Aggregate(keyValues.First().Key + "=" + keyValues.First().Value, (qs, property) => qs + "&" + property.Key + "=" + property.Value);
            return queryString;
        }
    }
}