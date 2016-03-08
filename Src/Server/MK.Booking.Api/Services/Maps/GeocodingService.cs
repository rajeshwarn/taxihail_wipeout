#region

using System.Globalization;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    /// <summary>
    ///     documentation https://developers.google.com/maps/documentation/geocoding/
    /// </summary>
    public class GeocodingService : BaseApiService
    {
        private readonly IGeocoding _geocoding;
        private readonly IAccountDao _accountDao;

        public GeocodingService(IGeocoding geocoding, IAccountDao accountDao)
        {
            _geocoding = geocoding;
            _accountDao = accountDao;
        }

        public Address[] Post(GeocodingRequest request)
        {
            if ((request.Lat.HasValue && request.Lng.HasValue && !request.Name.IsNullOrEmpty()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && request.Name.IsNullOrEmpty()))
            {
                //TODO MKTAXI-3918: Handle exceptions
                throw new HttpException((int)HttpStatusCode.BadRequest, "400"/*, "You must specify the name or the coordinate"*/);
            }

            // Get current language
            var language = CultureInfo.CurrentUICulture.Name;

            if (Session.IsAuthenticated())
            {
                var account = _accountDao.FindById(Session.UserId);
                if (account != null)
                {
                    language = account.Language;
                }
            }

            // ReSharper disable PossibleInvalidOperationException
            return request.Name.HasValue() 
                ? _geocoding.Search(request.Name, request.Lat, request.Lng, language, request.GeoResult) 
                : _geocoding.Search(request.Lat.Value, request.Lng.Value, language, request.GeoResult);
            // ReSharper restore PossibleInvalidOperationException
        }
    }
}