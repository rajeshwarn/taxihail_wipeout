using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class POIProvider : BaseService, IPOIProvider
    {
		public async Task<PickupPoint[]> GetPOIRefPickupList(string company, string textMatch, int maxRespSize)
        {
            try
            {
                return await UseServiceClientAsync<POIServiceClient, PickupPoint[]>(service => service.GetPOIRefPickupList(company, textMatch, maxRespSize ));
            }
            catch(Exception ex)
            {
				Logger.LogError(ex);

	            return new PickupPoint[0];
            }
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