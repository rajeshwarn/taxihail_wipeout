using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class CraftyClicksService : ICraftyClicksService
    {
        private readonly ICraftyClicksServiceClient _craftyClicksServiceClient;

        public CraftyClicksService(ICraftyClicksServiceClient craftyClicksServiceClient)
        {
            _craftyClicksServiceClient = craftyClicksServiceClient;
        }

        public async Task<Address[]> GetCraftyClicksAddressFromPostalCode(string postalCode)
        {
            var addressInformation = await _craftyClicksServiceClient.GetAddressInformation(postalCode);


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


        private string GenerateFullAddress(string line1, string line2, string town, string postcode)
        {
            return line2.HasValue() 
                ? string.Join(", ", line1, line2, town, postcode) 
                : string.Join(", ", line1, town, postcode);
        }
    }
}