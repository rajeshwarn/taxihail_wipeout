#region

using System;
using System.Globalization;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
        private readonly IAccountDao _accountDao;

        public GeocodingService(IGeocoding geocoding, IAccountDao accountDao)
        {
            _geocoding = geocoding;
            _accountDao = accountDao;
        }

        public object Post(GeocodingRequest request)
        {
            if ((request.Lat.HasValue && request.Lng.HasValue && !request.Name.IsNullOrEmpty()) ||
                (!request.Lat.HasValue && !request.Lng.HasValue && request.Name.IsNullOrEmpty()))
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "You must specify the name or the coordinate");
            }

            // Get current language
            var language = CultureInfo.CurrentUICulture.Name;

            if (this.GetSession() != null
                && this.GetSession().UserAuthId != null)
            {
                var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
                if (account != null)
                {
                    language = account.Language;
                }
            }

            if (request.Name.HasValue())
            {
                return _geocoding.Search(request.Name, language, request.GeoResult);
            }
// ReSharper disable PossibleInvalidOperationException
            return _geocoding.Search(request.Lat.Value, request.Lng.Value, language, request.GeoResult);
// ReSharper restore PossibleInvalidOperationException
        }
    }
}