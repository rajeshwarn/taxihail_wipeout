using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Entity;
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

		public async Task<Airline[]> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
            try
            {
				return await UseServiceClientAsync<POIServiceClient, Airline[]>(service => service.GetPOIRefAirLineList(company, textMatch, maxRespSize));
            }
            catch(Exception ex)
            {
				Logger.LogError(ex);

	            return new Airline[0];
            }
        }
    }
}