#region

using System;
using System.Globalization;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class NearbyPlacesService : Service
    {
        private readonly IPlaces _client;
        private readonly IAccountDao _accountDao;

        public NearbyPlacesService(IPlaces client, IAccountDao accountDao)
        {
            _client = client;
            _accountDao = accountDao;
        }

        public object Get(NearbyPlacesRequest request)
        {
            if (string.IsNullOrEmpty(request.Name)
                && request.IsLocationEmpty())
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            var language = CultureInfo.CurrentUICulture.Name;
            if (this.GetSession() != null)
            {
                var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
                if (account != null)
                {
                    language = account.Language;
                }
            }

            return _client.SearchPlaces(request.Name, request.Lat, request.Lng, request.Radius, language);
        }
    }
}