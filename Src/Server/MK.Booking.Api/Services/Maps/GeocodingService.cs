using System;
using System.Linq;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using System.Net;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Api.Services
{
    /// <summary>
    /// documentation https://developers.google.com/maps/documentation/geocoding/
    /// </summary>
    public class GeocodingService : RestServiceBase<GeocodingRequest>
    {
        private readonly IGeocoding _geocoding;        
        
        public GeocodingService(IGeocoding geocoding)
        {
            _geocoding = geocoding;
        }

        public override object OnGet(GeocodingRequest request)
        {

             if ((request.Lat.HasValue && request.Lng.HasValue && !request.Name.IsNullOrEmpty()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && request.Name.IsNullOrEmpty()))
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "You must specify the name or the coordinate");
            }

             if (request.Name.HasValue())
             {
                 return _geocoding.Search(request.Name);
             }
             else
             {
                 return _geocoding.Search(request.Lat.Value, request.Lng.Value);
             }

        }

     



    }
}
