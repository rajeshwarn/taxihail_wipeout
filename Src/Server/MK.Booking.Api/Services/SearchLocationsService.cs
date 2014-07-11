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

namespace apcurium.MK.Booking.Api.Services
{
    public class SearchLocationsService : Service
    {
        private readonly IAddresses _client;
        private readonly IAccountDao _accountDao;

        public SearchLocationsService(IAddresses client, IAccountDao accountDao)
        {
            _client = client;
            _accountDao = accountDao;
        }

        public object Post(SearchLocationsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.Search_Locations_NameRequired.ToString());
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
           
            return _client.Search(request.Name, request.Lat.GetValueOrDefault(), request.Lng.GetValueOrDefault(), language,
                request.GeoResult);
        }
    }
}