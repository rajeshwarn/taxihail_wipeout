using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using CMTPayment.Extensions;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class CraftyClicksService : IPostalCodeService
    {
        private readonly IAppSettings _settingsService;
        public CraftyClicksService(IAppSettings settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<Address[]> GetAddressFromPostalCode(string postalCode)
        {
            var client = GetClient();

            var addressInformation = await client.PostAsync(new CraftyClicksRequest
            {
                Postcode = postalCode,
                Key = _settingsService.Data.CraftyClicksApiKey
            });

			if (addressInformation == null)
			{
				return new Address[0];
			}

            return addressInformation.Delivery_points
                .Select(point => new Address()
                {
                    City = addressInformation.Town,
                    ZipCode = postalCode,
                    AddressLocationType = AddressLocationType.Unspeficied,
                    FullAddress = GenerateFullAddress(point.Line_1, point.Line_2, addressInformation.Town, addressInformation.Postcode),
                    Latitude = addressInformation.GeoCode.Lat,
                    Longitude = addressInformation.GeoCode.Lng,
                    FriendlyName = point.Line_1,
                    AddressType = "craftyclicks"
                })
                .ToArray();
        }


        private JsonServiceClient GetClient()
        {
            return new JsonServiceClient("http://pcls1.craftyclicks.co.uk/json/");
        }

        private string GenerateFullAddress(string line1, string line2, string town, string postcode)
        {
            return line2.HasValue() 
                ? string.Join(", ", line1, line2, town, postcode) 
                : string.Join(", ", line1, town, postcode);
        }
    }
}