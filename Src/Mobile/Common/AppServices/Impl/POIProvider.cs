using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class POIProvider : BaseService, IPOIProvider
    {
        public async Task<string> GetPOIRefPickupList(string company, string textMatch, int maxRespSize)
        {
            var result = string.Empty;
            try
            {
                result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOIRefPickupList(company, textMatch, maxRespSize ));
            }
            catch
            {
#if DEBUG
                result = "[{\"additionalFee\":\"0.00\",\"name\":\"Curbside\",\"id\":\"utog.Curbside\",\"type\":\"pickuppoint_utog\"},{\"additionalFee\":\"15.00\",\"name\":\"Meet and Greet\",\"id\":\"utog.Meet and Greet\",\"type\":\"pickuppoint_utog\"}]";
#endif
            }
            return result;
        }

        public async Task<string> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
            string result;
            try
            {
                result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOIRefAirLineList(company, textMatch, maxRespSize));
            }
            catch
            {
#if DEBUG
                result = "[{\"name\":\"Delta\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"}, {\"name\":\"Air Canada\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"American Airlines\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"Southwest\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"United\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"Alaska Airlines\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"iataCode\":\"-+\",\"icaoCode\":\"--+\",\"name\":\"U.S. Air\",\"callsign\":null,\"alias\":null,\"active\":false,\"id\":\"--+\",\"type\":\"airline\",\"openFlightsId\":\"13391\",\"homeCountry\":\"United States\"}]";
#endif
            }
            return result;
        }
    }
}