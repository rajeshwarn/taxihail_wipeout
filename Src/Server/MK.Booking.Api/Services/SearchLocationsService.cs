#region

using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SearchLocationsService : BaseApiService
    {
        private readonly IAddresses _client;
        private readonly IAccountDao _accountDao;

        public SearchLocationsService(IAddresses client, IAccountDao accountDao)
        {
            _client = client;
            _accountDao = accountDao;
        }

        public Task<Address[]> Post(SearchLocationsRequest request)
        {
            if (!request.Name.HasValueTrimmed())
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.Search_Locations_NameRequired.ToString());
            }

            var language = CultureInfo.CurrentUICulture.Name;
            if (Session.IsAuthenticated())
            {
                var account = _accountDao.FindById(Session.UserId);
                if (account != null && account.Language.HasValue())
                {
                    language = account.Language;
                }
            }
           
            return _client.SearchAsync(request.Name, request.Lat.GetValueOrDefault(), request.Lng.GetValueOrDefault(), language, request.GeoResult);
        }
    }
}