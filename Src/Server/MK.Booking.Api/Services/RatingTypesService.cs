using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class RatingTypesService: RestServiceBase<RatingTypesRequest>
    {
        private readonly IRatingTypeDao _dao;

        public RatingTypesService(IRatingTypeDao Dao)
        {
            _dao = Dao;
        }

        public override object OnGet(RatingTypesRequest request)
        {
            return _dao.GetAll();
        }
    }
}