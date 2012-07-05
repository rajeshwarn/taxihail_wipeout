using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using System.Net;
using System.Globalization;
using apcurium.MK.Booking.Api.Services.GoogleApi;

namespace apcurium.MK.Booking.Api.Services
{





    public class GeocodingService : RestServiceBase<GeocodingRequest>
    {



        public override object OnGet(GeocodingRequest request)
        {
            var client = new JsonServiceClient("http://maps.googleapis.com/maps/api/");

            var resource = CreateResourceFromRequest(request);

            var geoResult = client.Get<GeoResult>(resource);

            if (geoResult.Status == ResultStatus.OK)
            {
                var addresses =  geoResult.Results
                                            .Where( r => r.Formatted_address.HasValue() &&  
                                                         r.Geometry != null &&  r.Geometry.Location != null  &&
                                                         r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 && 
                                                         r.AddressComponentTypes.Any(type => type == AddressComponentType.Street_address))
                                            .Select(r => ConvertToAddress(r)).ToArray();

                return new AddressList{ Addresses = addresses };
            }
            else
            {
                return new AddressList();
            }
        }

        private Address ConvertToAddress(GeoObj obj)
        {
            return new Address { FullAddress = obj.Formatted_address, Id = Guid.Empty, Latitude = obj.Geometry.Location.Lat, Longitude = obj.Geometry.Location.Lng };
        }

        private string CreateResourceFromRequest(GeocodingRequest request)
        {
         
            if ( (request.Lat.HasValue && request.Lng.HasValue && request.Name.HasValue()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && !request.Name.HasValue()))
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "You must specify the name or the coordinate");
            }
            
            if (request.Name.HasValue())
            {
                return string.Format(  CultureInfo.InvariantCulture, "geocode/json?address={0},montreal,qc,canada&region=ca&sensor=false", request.Name.Split(' ').JoinBy("+"));
            }
            else
            {
                return string.Format( CultureInfo.InvariantCulture, "geocode/json?latlng={0},{1}&sensor=false", request.Lat, request.Lng);
            }

        }



    }
}
