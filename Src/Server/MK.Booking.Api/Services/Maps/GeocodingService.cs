#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    /// <summary>
    ///     documentation https://developers.google.com/maps/documentation/geocoding/
    /// </summary>
    public class GeocodingService :Service
    {
        private readonly IGeocoding _geocoding;

        public GeocodingService(IGeocoding geocoding)
        {
            _geocoding = geocoding;
        }

        public object Post(GeocodingRequest request)
        {
            if ((request.Lat.HasValue && request.Lng.HasValue && !request.Name.IsNullOrEmpty()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && request.Name.IsNullOrEmpty()))
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "You must specify the name or the coordinate");
            }

            if (request.Name.HasValue())
            {
                return _geocoding.Search(request.Name, request.GeoResult);
            }
// ReSharper disable PossibleInvalidOperationException
            return _geocoding.Search(request.Lat.Value, request.Lng.Value, request.GeoResult);
// ReSharper restore PossibleInvalidOperationException
        }
    }
}