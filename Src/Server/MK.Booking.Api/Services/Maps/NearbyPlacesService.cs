#region

using System.Globalization;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class NearbyPlacesService : BaseApiService
    {
        private readonly IPlaces _client;
        private readonly IAccountDao _accountDao;

        public NearbyPlacesService(IPlaces client, IAccountDao accountDao)
        {
            _client = client;
            _accountDao = accountDao;
        }

        public Address[] Get(NearbyPlacesRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) && request.IsLocationEmpty())
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            var language = CultureInfo.CurrentUICulture.Name;

            if (!Session.IsAuthenticated())
            {
                return _client.SearchPlaces(request.Name, request.Lat, request.Lng, language);
            }

            var account = _accountDao.FindById(Session.UserId);
            if (account != null)
            {
                language = account.Language;
            }

            return _client.SearchPlaces(request.Name, request.Lat, request.Lng, language);
        }
    }
}