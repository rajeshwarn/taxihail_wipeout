using System;
using System.Linq;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using System.Net;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Api.Services.Mappers;

namespace apcurium.MK.Booking.Api.Services
{
    /// <summary>
    /// documentation https://developers.google.com/maps/documentation/geocoding/
    /// </summary>
    public class GeocodingService : RestServiceBase<GeocodingRequest>
    {
        private readonly IMapsApiClient _client;
        private IConfigurationManager _configManager;
        private string[] _otherTypesAllowed = new string[] {"airport", "transit_station", "bus_station", "train_station"};
        public GeocodingService(IMapsApiClient client, IConfigurationManager configManager)
        {
            _client = client;
            _configManager = configManager;
        }

        public override object OnGet(GeocodingRequest request)
        {
            var filteredResult = GetResultUsingFilter(request, true);

            if (!filteredResult.Addresses.Any() )
            {
                return GetResultUsingFilter(request, false);
            }            
            else
            {
                return filteredResult;
            }
        }

        private AddressList GetResultUsingFilter(GeocodingRequest request, bool useFilter)
        {
            var geoResult = GetResultFromRequest(request, useFilter);

            if (geoResult.Status == ResultStatus.OK)
            {
                var addressFilter = _configManager.GetSetting("GeoLoc.AddressFilter");
                var googleResults = geoResult.Results.ToArray();

                var addresses = googleResults.Where(r => r.Formatted_address.HasValue() &&
                                                         (addressFilter.IsNullOrEmpty() || r.Formatted_address.ToLower().Contains(addressFilter.ToLower())) &&
                                                         r.Geometry != null && r.Geometry.Location != null &&
                                                         r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 &&
                                                         ( r.AddressComponentTypes.Any(type => type == AddressComponentType.Street_address) ||
                                                         ( r.Types.Any( t => _otherTypesAllowed.Any( o=>o.ToLower() == t.ToLower() ) ) ) ) )
                                            .Select(new GeoObjToAddressMapper().ConvertToAddress).ToArray();

                if (addresses.Count() == 0)
                {
                    addresses = googleResults.Where(r => r.Formatted_address.HasValue() &&
                                                         r.Geometry != null && r.Geometry.Location != null &&
                                                         r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 &&
                                                         (r.AddressComponentTypes.Any(type => type == AddressComponentType.Street_address) ||
                                                         ( r.Types.Any( t => _otherTypesAllowed.Any( o=>o.ToLower() == t.ToLower() ) ) ) ) )
                                            .Select(new GeoObjToAddressMapper().ConvertToAddress).ToArray();
                }

                return new AddressList { Addresses = addresses };
            }
            else
            {
                return new AddressList();
            }
        }



        private GeoResult GetResultFromRequest(GeocodingRequest request, bool useFilter)
        {

            if ((request.Lat.HasValue && request.Lng.HasValue && !request.Name.IsNullOrEmpty()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && request.Name.IsNullOrEmpty()))
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "You must specify the name or the coordinate");
            }

            if (request.Name.HasValue())
            {
                var filter = _configManager.GetSetting("GeoLoc.SearchFilter");

                if ((filter.HasValue()) && (useFilter))
                {
                    var filteredName = string.Format(filter, request.Name.Split(' ').JoinBy("+"));
                    return _client.GeocodeAddress(filteredName);
                }
                else
                {
                    return _client.GeocodeAddress(request.Name.Split(' ').JoinBy("+"));
                }
            }
            else
            {
                return _client.GeocodeLocation(request.Lat.GetValueOrDefault(), request.Lng.GetValueOrDefault());

            }

        }



    }
}
