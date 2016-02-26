using System.Linq;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using AutoMapper;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PopularAddressController : BaseApiController
    {
        private readonly IPopularAddressDao _popularAddressDao;

        public PopularAddressController(IPopularAddressDao popularAddressDao)
        {
            _popularAddressDao = popularAddressDao;
        }

        [HttpGet, NoCache, Route("popularaddresses")]
        public ClientPopularAddressResponse GetClientPopularAddress()
        {
            var address = _popularAddressDao.GetAll()
                .Select(Mapper.Map<Address>);

            return new ClientPopularAddressResponse(address);
        }

        [HttpGet, NoCache, Route("admin/popularaddresses")]
        public AdminPopularAddressResponse GetAdminPopularAddress()
        {
            return new AdminPopularAddressResponse(_popularAddressDao.GetAll());
        }
    }
}
