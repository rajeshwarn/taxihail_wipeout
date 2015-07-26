using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;
using System;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class POIProvider : BaseService, IPOIProvider
    {
        public POIProvider()
        {
        }

        public async Task<string> GetPOI(string locationType, string placeId)
        {
            var result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOI(locationType, placeId));
//            var result = "{\"id\":null,\"address\":{\"id\":\"00000000000000000000000000000000\",\"placeId\":\"49.210833^^^-57.391388^^^Deer Lake\",\"friendlyName\":\"Deer Lake\",\"streetNumber\":null,\"addressLocationType\":\"Airport\",\"street\":null,\"city\":\"Deer Lake\",\"zipCode\":null,\"state\":null,\"fullAddress\":\"Deer Lake (YDF), Deer Lake, Canada\",\"longitude\":-57.391388,\"latitude\":49.210833,\"apartment\":null,\"ringCode\":null,\"buildingName\":null,\"isHistoric\":false,\"favorite\":false,\"addressType\":null,\"displayAddress\":\"Deer Lake (YDF), Deer Lake, Canada\"},\"pickupLocations\":{\"Curbside\":\"0\",\"In Terminal\":\"15.00\",\"At Gate\":\"100.00\"},\"additionalQuestions\":{\"Airline\":\"reference:airline\",\"Flight Number\":\"\"},\"usePriceEngine\":true}";
//            var result = "{\"id\":null,\"address\":{\"id\":\"00000000000000000000000000000000\",\"pickupLocations\":{\"Curbside\":\"0\",\"In Terminal\":\"15.00\",\"At Gate\":\"100.00\"},\"additionalQuestions\":{\"Airline\":\"reference:airline\",\"Flight Number\":\"\"},\"usePriceEngine\":true}";
            return result;
        }

        public async Task<string> GetPOIRefInfo(string reference)
        {
            var result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOIRefInfo(reference));
            //var result = "[{\"type\":\"airline\",\"name\":\"South West Africa Territory Force\"},{\"type\":\"airline\",\"name\":\"U.S. Air\"},{\"type\":\"airline\",\"name\":\"Lombards Air\"},{\"type\":\"airline\",\"name\":\"Avilu\"},{\"type\":\"airline\",\"name\":\"AirOne Continental\"}]";
            return result;
        }

        public async Task<string> GetPOIRefPickupList(string company, string textMatch, int maxRespSize)
        {
            var result = string.Empty;
            try
            {
                result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOIRefPickupList(company, textMatch, maxRespSize ));
            }
            catch
            {
                result = string.Empty;
#if DEBUG
                result = "[{\"additionalFee\":\"0.00\",\"name\":\"Curbside\",\"id\":\"utog.Curbside\",\"type\":\"pickuppoint_utog\"},{\"additionalFee\":\"15.00\",\"name\":\"Meet and Greet\",\"id\":\"utog.Meet and Greet\",\"type\":\"pickuppoint_utog\"}]";
#endif
            }
            return result;
        }

        public async Task<string> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize)
        {
            var result = string.Empty;
            try
            {
//                Log.Debug("MK-Dbg", String.Format("POIProvider::GetPOIRefAirLineList: company:{0}, textMatch:{1}, maxRespSize:{2}", company, textMatch, maxRespSize));
                result = await UseServiceClientAsync<POIServiceClient, string>(service => service.GetPOIRefAirLineList(company, textMatch, maxRespSize));
            }
            catch
            {
                result = string.Empty;
#if DEBUG
                result = "[{\"name\":\"Delta\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"}, {\"name\":\"Air Canada\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"American Airlines\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"Southwest\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"United\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"name\":\"Alaska Airlines\",\"callsign\":null,\"id\":\"***\",\"type\":\"airline\"},{\"iataCode\":\"-+\",\"icaoCode\":\"--+\",\"name\":\"U.S. Air\",\"callsign\":null,\"alias\":null,\"active\":false,\"id\":\"--+\",\"type\":\"airline\",\"openFlightsId\":\"13391\",\"homeCountry\":\"United States\"}]";
#endif
            }
            return result;
        }
    }
}