using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.CraftyClicks.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using MK.Booking.Google.Android.Extensions;
using ModernHttpClient;

namespace apcurium.MK.Booking.MapDataProvider.CraftyClicks
{
    public class CraftyClicksService : IPostalCodeService
    {
        private const string UkPostcodeRegexPattern = "^[A-Za-z][A-Za-z]?[0-9][0-9]?[A-Za-z]?\\s?[0-9][A-Za-z][A-Za-z;]$";
        private readonly IAppSettings _settingsService;


        public CraftyClicksService(IAppSettings settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<Address[]> GetAddressFromPostalCodeAsync(string postalCode)
        {
            var client = GetClient();

            var addressInformation = await client.PostAsync<CraftyClicksAddress>("/rapidaddress", new CraftyClicksRequest
            {
                Postcode = postalCode,
                Key = _settingsService.Data.CraftyClicksApiKey
            });

            return ProcessAddressInformation(postalCode, addressInformation);
        }

        private Address[] ProcessAddressInformation(string postalCode, CraftyClicksAddress addressInformation)
        {
            if (addressInformation == null || addressInformation.Delivery_points == null)
            {
                return new Address[0];
            }

            return addressInformation.Delivery_points
                .Select(point => new Address()
                {
                    City = addressInformation.Town,
                    ZipCode = postalCode,
                    AddressLocationType = AddressLocationType.Unspeficied,
                    FullAddress =
                        GenerateFullAddress(point.Line_1, point.Line_2, addressInformation.Town, addressInformation.Postcode),
                    Latitude = addressInformation.GeoCode.Lat,
                    Longitude = addressInformation.GeoCode.Lng,
                    FriendlyName = point.Line_1,
                    AddressType = "craftyclicks"
                })
                .ToArray();
        }

        //This should only be used on the server.
        public Address[] GetAddressFromPostalCode(string postalCode)
        {
            return GetAddressFromPostalCodeAsync(postalCode).Result;
        }

        public bool IsValidPostCode(string postalCode)
        {
            return postalCode.HasValue() && Regex.IsMatch(postalCode, UkPostcodeRegexPattern);
        }


        private HttpClient GetClient()
        {
            var client = new HttpClient(new NativeMessageHandler())
            {
#if DEBUG
                BaseAddress = new Uri("http://pcls1.craftyclicks.co.uk/json/")
#else
                BaseAddress = new Uri("https://pcls1.craftyclicks.co.uk/json/");
#endif
            };
            
            return client;
        }

        private string GenerateFullAddress(string line1, string line2, string town, string postcode)
        {
            return line2.HasValue() 
                ? string.Join(", ", line1, line2, town, postcode) 
                : string.Join(", ", line1, town, postcode);
        }
    }
}